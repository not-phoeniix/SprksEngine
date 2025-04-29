using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

/// <summary>
/// A static linear level bar, inherits from MenuElement
/// </summary>
public class LevelBar : MenuElement {
    private readonly SpriteElement icon;

    /// <summary>
    /// Gets/sets the value of this level bar
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// Gets/sets the maximum value of this level bar
    /// </summary>
    public float MaxValue { get; set; }

    /// <summary>
    /// Gets/sets the minimum value of this level bar
    /// </summary>
    public float MinValue { get; set; }

    /// <summary>
    /// Creates a new level bar
    /// </summary>
    /// <param name="bounds">Bounds of bar to create, marginless and top-left aligned</param>
    /// <param name="style">Style of level bar</param>
    /// <param name="icon">Icon to show next to this level bar</param>
    public LevelBar(Rectangle bounds, SpriteElement icon, ElementStyle style)
    : base(bounds, style) {
        Value = 0.5f;
        MaxValue = 1.0f;
        MinValue = 0.0f;
        this.icon = icon;
        this.icon.Style.XAlignment = XAlign.Left;
        this.icon.Style.YAlignment = YAlign.Center;
    }

    /// <summary>
    /// Updates the logic for this level bar
    /// </summary>
    /// <param name="dt">Time passed since frame</param>
    public override void Update(float dt) {
        if (icon != null) {
            Rectangle spriteBounds = MarginlessBounds;
            spriteBounds.Inflate(-Style.Padding, -Style.Padding);
            spriteBounds.Width = icon.Bounds.Width;

            icon.Position = new(
                spriteBounds.Left,
                spriteBounds.Center.Y
            );
        }
    }

    /// <summary>
    /// Draws this level bar to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        // greyed out full progress bar
        Rectangle fullBarRect = MarginlessBounds;
        fullBarRect.Inflate(-Style.Padding, -Style.Padding);

        // offset bar if icon exists by icon's horizontal bounds
        if (icon != null) {
            fullBarRect.X += icon.Bounds.Width;
            fullBarRect.Width -= icon.Bounds.Width;
        }

        // percent of 50 from 0-100 would be 0.5
        // percent of 0 from -1 - 1 would be 0.5
        // percent of 2 from -1 - 3 would be 0.75
        // etc
        float percent = (Value - MinValue) / (MaxValue - MinValue);
        Rectangle progressRect = fullBarRect;
        progressRect.Width = (int)MathF.Floor(progressRect.Width * percent);

        sb.DrawRectFill(MarginlessBounds, Style.BackgroundColor);
        sb.DrawRectFill(fullBarRect, Style.InactiveColor);
        sb.DrawRectFill(progressRect, Style.ForegroundColor);
        icon?.Draw(sb);
    }
}
