namespace FinanzasPersonales.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando una entidad no es encontrada.
/// Principio: Single Responsibility - Solo maneja casos de entidades no encontradas
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityName { get; set; }
    public string EntityId { get; set; }

    public EntityNotFoundException(string entityName, string entityId)
        : base($"La entidad '{entityName}' con id '{entityId}' no fue encontrada.", "ENTITY_NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityName, string entityId, string id)
        : base($"La entidad '{entityName}' con {id} '{entityId}' no fue encontrada.", "ENTITY_NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
