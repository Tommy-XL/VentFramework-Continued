namespace VentLib.Options.Interfaces;

public interface ITabEvent : IControllerEvent
{
    IGameOptionTab? Source();
}