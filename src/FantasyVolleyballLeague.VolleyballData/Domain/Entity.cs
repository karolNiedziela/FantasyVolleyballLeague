namespace FantasyVolleyballLeague.Api.Domain
{
    internal abstract class Entity
    {
        protected Entity()
        {
            Id = EntityId.New();
        }

        public Guid Id { get; private set; }
    }
}
