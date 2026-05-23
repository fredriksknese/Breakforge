using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakforge.Content;
using Breakforge.Core;
using Breakforge.Definitions;
using Breakforge.Player;
using Breakforge.Scenes;

namespace Breakforge;

public sealed class BreakforgeGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _gfx;
    private SpriteBatch _sb = null!;
    private readonly Registry _registry = new();
    private readonly PlayerProfile _profile;
    private readonly SceneManager _scenes = new();
    private const int VirtualW = 1024;
    private const int VirtualH = 720;

    // Auto-save debounce: wait this long after the last mutation before
    // flushing to disk, so a burst of changes coalesces into one write.
    private const float AutoSaveDebounceSeconds = 1.0f;
    private float _saveCountdown;

    /// <summary>
    /// Static accessor used by content effects (e.g. MapUnlockEffect) that need
    /// the active profile. Set when the game constructs.
    /// </summary>
    public static PlayerProfile? ActiveProfile { get; private set; }

    public BreakforgeGame()
    {
        _gfx = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = VirtualW,
            PreferredBackBufferHeight = VirtualH,
            SynchronizeWithVerticalRetrace = true,
        };
        IsMouseVisible = true;
        Window.Title = "Breakforge — forge your build, break the bricks";
        Content.RootDirectory = "Content";
        _profile = ProfileStorage.Load();
        ActiveProfile = _profile;
    }

    protected override void Initialize()
    {
        BrickCatalog.Register(_registry);
        PowerupCatalog.Register(_registry);
        MapCatalog.Register(_registry);
        SkillCatalog.Register(_registry);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sb = new SpriteBatch(GraphicsDevice);
        Primitives.Init(GraphicsDevice);
        _scenes.Goto(new MainMenuScene());
    }

    private SceneContext BuildContext() => new()
    {
        Registry = _registry,
        Profile = _profile,
        Scenes = _scenes,
        ViewportBounds = new Rectangle(0, 0, VirtualW, VirtualH),
    };

    protected override void Update(GameTime gameTime)
    {
        var ctx = BuildContext();
        _scenes.TickTransitions(ctx);
        _scenes.Current?.Update(ctx, gameTime);
        TickAutoSave((float)gameTime.ElapsedGameTime.TotalSeconds);
        base.Update(gameTime);
    }

    private void TickAutoSave(float dt)
    {
        if (_profile.IsDirty)
        {
            // Reset the debounce on every dirty tick so a stream of changes
            // (e.g. score climbing as bricks shatter) doesn't flush every frame.
            _saveCountdown = AutoSaveDebounceSeconds;
            _profile.ClearDirty();
        }
        else if (_saveCountdown > 0f)
        {
            _saveCountdown -= dt;
            if (_saveCountdown <= 0f) ProfileStorage.Save(_profile);
        }
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        // Flush anything that the debounce hasn't gotten to yet.
        if (_profile.IsDirty || _saveCountdown > 0f) ProfileStorage.Save(_profile);
        base.OnExiting(sender, args);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(8, 10, 18));
        _sb.Begin(samplerState: SamplerState.PointClamp);
        _scenes.Current?.Draw(BuildContext(), _sb);
        _sb.End();
        base.Draw(gameTime);
    }
}
