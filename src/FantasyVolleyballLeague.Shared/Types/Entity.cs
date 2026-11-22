namespace FantasyVolleyballLeague.Shared.Types
{
    public abstract class Entity
    {
        protected Entity()
        {
            Id = EntityId.New();
        }

        public Guid Id { get; private set; }
    }
}
