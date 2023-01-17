#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;
using InnerNet;
using MonoMod.RuntimeDetour;
using UnityEngine;
using VentLib.Extensions;
using VentLib.Logging;
using VentLib.RPC.Interfaces;
using VentLib.Utilities;

namespace VentLib.RPC;

internal class RpcHookHelper
{
    internal static long globalSendCount;
    private static List<DetouredSender> _senders = new();

    private static readonly OpCode[] _ldc = { OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8 };
    private static readonly OpCode[] _ldarg = { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

    internal static Hook Generate(ModRPC modRPC)
    {
        MethodInfo executingMethod = modRPC.TargetMethod;
        Type[] parameters = executingMethod.GetParameters().Select(p => p.ParameterType).ToArray();

        DynamicMethod m = new(
            executingMethod.Name,
            executingMethod.ReturnType,
            parameters);

        int senderSize = _senders.Count;

        ILGenerator ilg = m.GetILGenerator();
        if (senderSize <= 8)
            ilg.Emit(_ldc[senderSize]);
        else
            ilg.Emit(OpCodes.Ldc_I4_S, senderSize);
        ilg.Emit(OpCodes.Call, AccessTools.Method(typeof(RpcHookHelper), nameof(GetSender)));

        if (parameters.Length <= 8)
            ilg.Emit(_ldc[parameters.Length]);
        else
            ilg.Emit(OpCodes.Ldc_I4_S, parameters.Length);
        ilg.Emit(OpCodes.Newarr, typeof(object));

        for (int i = 0; i < parameters.Length; i++)
        {
            ilg.Emit(OpCodes.Dup);
            if (i <= 8)
                ilg.Emit(_ldc[i]);
            else
                ilg.Emit(OpCodes.Ldc_I4_S, i);

            if (i <= 3)
                ilg.Emit(_ldarg[i]);
            else
                ilg.Emit(OpCodes.Ldarg_S, i);
            if (parameters[i].IsPrimitive)
                ilg.Emit(OpCodes.Box, parameters[i]);
            ilg.Emit(OpCodes.Stelem_Ref);
        }

        ilg.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(DetouredSender), nameof(DetouredSender.IntermediateSend)));
        ilg.Emit(OpCodes.Ret);

        _senders.Add(new DetouredSender(modRPC));
        return new Hook(executingMethod, m);
    }

    private static DetouredSender GetSender(int index) => _senders[index];

}

public class DetouredSender
{
    private readonly int uuid = UnityEngine.Random.RandomRangeInt(0, 999999);
    private int localSendCount;
    private readonly ModRPC modRPC;
    private readonly uint callId;
    private readonly RpcActors senders;
    private readonly RpcActors receivers;

    internal DetouredSender(ModRPC modRPC)
    {
        this.modRPC = modRPC;
        callId = this.modRPC.CallId;
        senders = modRPC.Senders;
        receivers = this.modRPC.Receivers;
        modRPC.Sender = this;
    }

    internal void IntermediateSend(params object?[] args)
    {
        if (modRPC.Invocation is MethodInvocation.ExecuteBefore) modRPC.InvokeTrampoline(args!);
        Send(null, args);
        if (modRPC.Invocation is MethodInvocation.ExecuteAfter) modRPC.InvokeTrampoline(args!);
    }

    private async void DelayedSend(int[]? targets, object?[] args)
    {
        int retries = 0;
        while ((AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null) && retries < 50)
        {
            await Task.Delay(100);
            retries++;
        }

        if (retries >= 50)
            throw new TimeoutException("Could not gather client instance");
        Send(targets, args);
    }

    internal void Send(int[]? targets, object?[] args)
    {
        if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null) {
            DelayedSend(targets, args);
            return;
        }
        if (!CanSend(out int[]? lastSender) || !Vents.CallingAssemblyFlag().HasFlag(VentControlFlag.AllowedSender)) return;
        lastSender ??= targets;
        RealSend(lastSender, args);
    }

    private void RealSend(int[]? targets, object?[] args)
    {
        RpcHookHelper.globalSendCount++; localSendCount++;
        RpcV2 v2 = RpcV2.Immediate(PlayerControl.LocalPlayer.NetId, 203).Write(callId).RequireHost(false);
        v2.Write((byte)receivers);
        v2.WritePacked(PlayerControl.LocalPlayer.NetId);
        args.Do(a => WriteArg(v2, a!));
        int[]? blockedClients = Vents.CallingAssemblyBlacklist();

        string senderString = AmongUsClient.Instance.AmHost ? "Host" : "NonHost";
        int clientId = PlayerControl.LocalPlayer.GetClientId();
        if (targets != null) {
            VentLogger.Debug($"(Client: {clientId}) Sending RPC ({callId}) as {senderString} to {targets.StrJoin()} | ({senders} | {args} | {localSendCount}::{uuid}::{RpcHookHelper.globalSendCount}", "DetouredSender");
            v2.SendInclusive(blockedClients == null ? targets : targets.Except(blockedClients).ToArray());
        } else if (blockedClients != null) {
            VentLogger.Debug($"(Client: {clientId}) Sending RPC ({callId}) as {senderString} to all except {blockedClients.StrJoin()} | ({senders} | {args} | {localSendCount}::{uuid}::{RpcHookHelper.globalSendCount}", "DetouredSender");
            v2.SendExclusive(blockedClients);
        } else {
            VentLogger.Debug($"(Client: {clientId}) Sending RPC ({callId}) as {senderString} to all | ({this.senders} | {args} | {localSendCount}::{uuid}::{RpcHookHelper.globalSendCount}", "DetouredSender");
            v2.Send();
        }
    }

    private bool CanSend(out int[]? targets)
    {
        targets = null;
        if (receivers is RpcActors.LastSender) targets = new[] { Vents.GetLastSender(callId)?.GetClientId() ?? 999 };

        return senders switch
        {
            RpcActors.None => false,
            RpcActors.Host => AmongUsClient.Instance.AmHost,
            RpcActors.NonHosts => !AmongUsClient.Instance.AmHost,
            RpcActors.LastSender => true,
            RpcActors.Everyone => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    internal static void WriteArg(RpcV2 rpcV2, object arg)
    {
        RpcV2 _ = (arg) switch
        {
            bool data => rpcV2.Write(data),
            byte data => rpcV2.Write(data),
            float data => rpcV2.Write(data),
            int data => rpcV2.Write(data),
            sbyte data => rpcV2.Write(data),
            string data => rpcV2.Write(data),
            uint data => rpcV2.Write(data),
            ulong data => rpcV2.Write(data),
            ushort data => rpcV2.Write(data),
            Vector2 data => rpcV2.Write(data),
            InnerNetObject data => rpcV2.Write(data),
            IRpcWritable data => rpcV2.Write(data),
            _ => WriteArgNS(rpcV2, arg)
        };
    }

    private static RpcV2 WriteArgNS(RpcV2 rpcV2, object arg)
    {
        switch (arg)
        {
            case IEnumerable enumerable:
                List<object> list = enumerable.Cast<object>().ToList();
                rpcV2.Write((ushort)list.Count);
                foreach (object obj in list)
                    WriteArg(rpcV2, obj);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Invalid Argument: {arg}");
        }

        return rpcV2;
    }
}

