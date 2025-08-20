public class BoostJetsPerk : IActivePerk
{
    private readonly BoostJetsSO _so;
    private bool _usedThisTurn = false;

    public BoostJetsPerk(BoostJetsSO so) { _so = so; }

    public string Name => _so.perkName;
    public int    Tier => _so.tier;
    public int    Cost => _so.cost;

    public bool CanActivate(PlayerShip ship)
    {
        // only in Move mode, only once per turn, and enough move points
        return ship.currentMode == PlayerShip.PlayerActionMode.Move
            && !_usedThisTurn
            && ship.movesRemainingThisRound >= Cost;
    }

    public void Activate(PlayerShip ship)
    {
        _usedThisTurn = true;

        // consume your cost
        //ship.movesRemainingThisRound -= Cost;
        GameManager.Instance.UpdateFightingUI_AtRoundStart();

        // arm the “skip end of turn” for exactly one move
        ship.skipNextMoveEndsTurn = true;
    }
}

