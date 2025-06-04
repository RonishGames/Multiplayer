using Unity.Netcode.Components;

public class ClintNetworkIteam : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }   
}
