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
  
# Motivation
Creating EF Core entity classes by hand is pretty, but there is a fair amount of boilerplate -- particularly in the `IEntityTypeConfiguration<T>` classes. I find the syntax around foreign keys kind of verbose, for example, but probably unavoidable. The goal here is to see if a "markdown-style" way to create this code is productive and workable for real apps.

This came up suddenly while working on a new app idea, when I found it was really tedious to create all the entity classes by hand. You can of course scaffold out EF Core entities from an existing database. I have done that and that is a great feature. But I'm looking for a faster, more fluid experience of initial creation. In this particular situation, if I scaffolded an existing database, I'd have to modify the original to achieve some of the structural changes I'm after. It's a lot easier to conceptualize a greenfield/cleansheet design than to _rework_ an old design. That's why EF Core scaffolding was not a fit here.

# WPF App
My original vision was to have text-only markdown files in your solution that are "compiled" by a custom tool. But a dedicated GUI makes navigation and viewing of inputs and outputs more straightforward.

<img width="993" height="592" alt="image" src="https://github.com/user-attachments/assets/61ea7206-f9d1-4e20-8d08-799be67637d3" />

<img width="308" height="181" alt="image" src="https://github.com/user-attachments/assets/2b281e13-6846-46a4-a515-06110748105c" />

<img width="688" height="310" alt="image" src="https://github.com/user-attachments/assets/8fdfb494-300b-4b4a-a572-a8ba59970904" />

# AI Assistance
Quite a lot of this was "vibe coded". If you want to look through the closed issues and pull requests on this, you can see how I approached it.

