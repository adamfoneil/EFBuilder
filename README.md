This is intended as faster way to create EF Core entity classes, using a "markdown" style syntax that is transpiled into C# source files.

An entity definition works like this:
- the first line of a definition is the entity name, which will become the C# class name. It may optionally have a base class which will use regular C# base class syntax, a colon followed by another class name
- properties are all additional lines after the first line
- properties have this pattern:
```
[#]Name [ClrTypeOrReferencedEntity[?]] [<ParentCollection] [= default value] [// comment]
```
- a leading hash sign indicates the column is part of a unique constraint
- the data type may be either a CLR type followed by a size in parentheses, e.g. `string(50)`
- the referenced entity can be another entity name in scope, which is returned by an `IEntityEnumerator` instance
- if no CLR type or entity is specified, then try to match the property name with an existing entity. Trim any suffix like "Id" to enable the match
- if there is a known referenced entity, the less than sign is used to define the parent collection in the referenced entity (via an ICollection<T> navigation property)
- when a referenced entity is present, an additional navigation property is generated that removes the "Id" suffix, and is always nullable
- there can be an optional default value, prefixed with an equal sign (such as `IsActive bool = true` would mean a bool field true by default
- there can be an optional comment on the end preceded by two slashes, like you'd find in C#
  
