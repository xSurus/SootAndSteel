using System.Collections.Generic;
using Gamelab;
using MonoGameGum.GueDeriving;

namespace Gamelab.Components;

partial class HubOverlay
{
    partial void CustomInitialize()
    {
    }

    public void Update(HashSet<int> readyPlayers, IReadOnlyList<int> joinedPlayerIndices)
    {
        int currentCredits = GamelabGame.Instance.CurrentRun.Credits;
        if (CurrencyDisplayInstance != null)
            CurrencyDisplayInstance.AmountText = currentCredits.ToString();

        if (PlayersReadyInstance != null)
        {
            PlayersReadyInstance.ReadyState = AreAllPlayersReady(readyPlayers, joinedPlayerIndices)
                ? PlayersReady.Ready.allReady
                : PlayersReady.Ready.notReady;
        }

        SyncReadySprites(readyPlayers, joinedPlayerIndices);
    }

    private static bool AreAllPlayersReady(HashSet<int> readyPlayers, IReadOnlyList<int> joinedPlayerIndices)
    {
        if (joinedPlayerIndices.Count == 0)
            return false;
        for (int i = 0; i < joinedPlayerIndices.Count; i++)
        {
            if (!readyPlayers.Contains(joinedPlayerIndices[i]))
                return false;
        }
        return true;
    }

    private void SyncReadySprites(HashSet<int> readyPlayers, IReadOnlyList<int> joinedPlayerIndices)
    {
        if (PlayersReadyInstance?.BluePlayer == null)
            return;

        SpriteRuntime[] byColorSlot =
        [
            PlayersReadyInstance.BluePlayer,
            PlayersReadyInstance.RedPlayer,
            PlayersReadyInstance.BrownPlayer,
            PlayersReadyInstance.YellowPlayer
        ];

        SetIconVisibility(false);

        for (int i = 0; i < joinedPlayerIndices.Count; i++)
        {
            int playerIndex = joinedPlayerIndices[i];
            if (!readyPlayers.Contains(playerIndex))
                continue;
            if ((uint)i >= (uint)byColorSlot.Length)
                continue;

            SpriteRuntime sprite = byColorSlot[i];
            if (sprite != null)
                sprite.Visible = true;
        }
    }

    private void SetIconVisibility(bool visible)
    {
        if (PlayersReadyInstance?.BluePlayer != null) PlayersReadyInstance.BluePlayer.Visible = visible;
        if (PlayersReadyInstance?.RedPlayer != null) PlayersReadyInstance.RedPlayer.Visible = visible;
        if (PlayersReadyInstance?.BrownPlayer != null) PlayersReadyInstance.BrownPlayer.Visible = visible;
        if (PlayersReadyInstance?.YellowPlayer != null) PlayersReadyInstance.YellowPlayer.Visible = visible;
    }
}
