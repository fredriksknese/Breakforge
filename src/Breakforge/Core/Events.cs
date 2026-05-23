using Microsoft.Xna.Framework;

namespace Breakforge.Core;

public readonly record struct BallHitBrickEvent(Entity Ball, Entity Brick, Vector2 ContactNormal) : IGameEvent;
public readonly record struct BallHitPaddleEvent(Entity Ball, Entity Paddle) : IGameEvent;
public readonly record struct BallHitWallEvent(Entity Ball, Vector2 Normal) : IGameEvent;
public readonly record struct BrickDamagedEvent(Entity Brick, int Amount, Entity? Source) : IGameEvent;
public readonly record struct BrickDestroyedEvent(Entity Brick, Entity? Source) : IGameEvent;
public readonly record struct PowerupCollectedEvent(Entity Powerup, Entity Paddle, string PowerupId) : IGameEvent;
public readonly record struct BallLostEvent(Entity Ball) : IGameEvent;
public readonly record struct LevelClearedEvent : IGameEvent;
public readonly record struct AreaDamageEvent(Vector2 Center, float Radius, int Amount, Entity? Source) : IGameEvent;
public readonly record struct ScoreChangedEvent(int Delta, int Total) : IGameEvent;
public readonly record struct LivesChangedEvent(int Lives) : IGameEvent;
public readonly record struct BrickCritEvent(Entity Brick, int Damage) : IGameEvent;
public readonly record struct BrickOverkillEvent(Entity Brick, int Leftover, Entity? Source) : IGameEvent;
public readonly record struct GoldGainedEvent(int Amount, Vector2 At) : IGameEvent;
public readonly record struct GemDroppedEvent(Entity Brick, int Value) : IGameEvent;
public readonly record struct LifeStolenEvent : IGameEvent;
public readonly record struct ShieldConsumedEvent(Entity Owner) : IGameEvent;
