public interface IActivePerk
{
    string Name { get; }
    int Tier { get; }
    int Cost { get; }                  // <-- new
    bool CanActivate(PlayerShip ship);
    void Activate(PlayerShip ship);
}
