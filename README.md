# Indigo ECS Code Generator

This is a sister repository to my [IndigoECS](https://github.com/rdshack/IndigoECS) game development library. It is used to generate necessary interface implementations used by the core library, using a schema that defines the state and inputs used for your specific game.

This code generator consumes a set of simple schema files, and then generates implementations for:
- Component Classes: All the component data containers used in your game.
- IComponentDefinitions: Responsible for defining attributes of the different component types used in your game
- IAliasLookup: Responsible for defining the alias's used in your game, as well as defining the input types
- IFrameSerializer: Responsible for serializing and deserializing the game state
- IComponentFactory: Responsible for creating new component data used in your specific game and pooling said data
- IWorldLogger: Responsible for lots of useful logging tools

## Schema Files
The code generator will run over all ".schema" files found in the target folder passed in as a program argument. It will then generate its code based on those schema files to the output folder passed in as the second program argument. You may put all your compononens, alias, enum, or any other type declaration all in a single schema file, or split them up into different files for organization.

## Component Declaration
Components are declared using the following format:
```
component name
{  
  fieldName1: fieldType;
  fieldName2 : fieldType;
}
```

For example:
```
component VelocityComponent
{  
  veloX: int;
  veloY: int;
  maxSpeed : int;
}
```

### Component Field Types
The following field types are suppported by the generator (not case sensitive):
- int
- float
- string
- entityId: serializes to an unsigned long
- enum
- fix64: 64-bit fixed point number. 32 bits for each side of decimal. Use this instead of float if you want to use any deterministic rollback featues.
- fix64Vec2
- fix64Vec3

### Component Key Field
A component can be given a single "key field" like so:
```
component PlayerOwnedComponent
{  
  [key]
  playerId: int;
}
```

A key field generates the ability to query components of that type based on the value of the given key field. Currently only a single key field is supported, but in the future I plan to support multiple key fields for more complex field-based queries.

### Component Attributes
The following attributes can be attached to a component type:
- Singleton: a singleton component is one per-simulation, rather than one per-component. This changes the access pattern for accessing these component fields to be more effiecient and more convenient
- SingleFrame: single frame components are automatically cleaned up at the beginning of each frame. This means components marked with this will never persist frame to frame. This is useful for things like event components.

How to attach component attributes:
```
[attributeType]
component ComponentName
{
	...
}
```

Some examples:
```
[singleton]
component GameComponent
{
  gamePhase : GamePhase;
  leftPlayerScore : int;
  rightPlayerScore : int;
  leftPlayerPawn : entityid;
  rightPlayerPawn : entityid;
  ball : entityid;
  mostRecentPointWasLeft : bool;
}

[singleframe]
component CollisionEventComponent
{
  collisionObjAEntity : entityid;
  collisionObjBEntity : entityid;
  collisionObjAPos : fix64vec2;
  collisionObjBPos : fix64vec2;
  collisionType : CollisionType;
  staticCollisionType : StaticColliderType;
}
```

## Enum Declaration
In order to use an enum as a field type, it must be declared in the schema file as well:
```
enum EnumName
{
  Val1,
  Val2
}
```

For example:
```
enum StaticGeoCollisionResponse
{
  Slide,
  Bounce
}

component CircleColliderComponent
{  
  radius : int;
  geoCollisionResponse : StaticGeoCollisionResponse;
}
```

## Alias Declaration
An alias is a name for a set of component types. These create easier (and more efficient) ways to query entity state.
```
alias AliasName
{
  ComponentType1,
  ComponentType2
}
```
ForExample
```
alias BouncyBall
{
  PositionComponent,
  RotationComponent,
  VelocityComponent,
  RotationVelocityComponent,
  BallMovementComponent,
  GravityComponent,
  CircleColliderComponent,
  TimeComponent
}
```

## Running The Generator
After compiling the program, simply run the program with the following arguments:
```
ecs_gen schema_source_folder output_folder
```

For example:
```
ecs_gen ../SlimeBallEcs/SlimeBall/schemas ../SlimeBallEcs/SlimeBall/Generated 
```

or, with full paths:

```
ecs_gen C:\code\SlimeBallEcs\schemas C:\code\SlimeBallEcs\Generated
```

Keep in mind, your schema source folder can be anywhere. I'd recommend putting it in your game projects directory (likely not anywhere near the generator's project code). This will make re-generating your component code painless as you can edit the schemas within your game's dev environment more easily.
