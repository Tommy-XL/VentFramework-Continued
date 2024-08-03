using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using InnerNet;
using VentLib.Logging.Default;
using VentLib.Networking.Handshake;
using VentLib.Version;
using VentLib.Version.BuiltIn;

namespace VentLib.Networking.RPC.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public class HandleRpcPatch
{
    internal static HashSet<string> AumUsers = new();
    private const uint VersionCheck = (uint)VentCall.VersionCheck;
    private static ModRPC _modRPC = Vents.FindRPC(VersionCheck, typeof(VersionCheck), nameof(Handshake.VersionCheck.SendVersion))!;

    public static bool Prefix(InnerNetObject __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        // Among Us Menu is a problem so we handle it within the library, change your call ID I dare you
        if (callId is NetworkRules.AmongUsMenuCallId)
        {
            // One issue is that SickoMenu can send RPCs pretending to be another player.
            byte playerId = reader.ReadByte();
            PlayerControl? player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.NetId == __instance.NetId);
            if (player == null) return false;
            if (playerId != player.PlayerId)
            {
                return false; // Fake AUM detection.
            }
            //if (AumUsers.Add(player.FriendCode)) log.SendInGame($"{player.Data.PlayerName} has joined the lobby with AmongUsMenu.");
            Vents.LastSenders[(uint)VentCall.VersionCheck] = player;
            _modRPC.InvokeTrampoline(new AmongUsMenuVersion());
            return false;
        } 
        // You can disable this in the SickoMenu settings. so just kinda hope they dont lol.
        else if (callId is NetworkRules.SickoMenuCallId)
        {
            PlayerControl? player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.NetId == __instance.NetId);
            if (player == null) return false;
            byte playerId = player.PlayerId;
            Vents.LastSenders[(uint)VentCall.VersionCheck] = player;
            _modRPC.InvokeTrampoline(new SickoMenuVersion());
            return false;
        } 
        else if (callId is NetworkRules.PrivateMessageCallId)
        {
            // Private Message Attempt. We'll just log it here bu 
            string playerName = reader.ReadString();
            string message = reader.ReadString();
            PlayerControl? player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.NetId == __instance.NetId);
            if (player == null) NoDepLogger.Debug($"{playerName} sent an AUM message saying: {message}. (we could not find the real player. so we are using their name in the rpc.)");
            else NoDepLogger.Debug($"{player.Data.name} sent an AUM message saying: {message}.");
            return false;
        }

        if (callId is not (203 or 204)) return true;
        RpcManager.HandleRpc(callId, reader);
        return false;
    }
}