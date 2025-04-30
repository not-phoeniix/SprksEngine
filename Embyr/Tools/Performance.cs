using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Embyr.Tools;

/// <summary>
/// Static helper class that deals with performance logging
/// </summary>
public static class Performance {
    // fixed update logic learned from here:
    //   https://gafferongames.com/post/fix_your_timestep/

    private static readonly Stopwatch frametimeStopwatch = new();
    private static readonly Stopwatch updateStopwatch = new();
    private static readonly Stopwatch drawStopwatch = new();
    private static readonly FpsCounter fps = new();
    private static readonly float averageIntervalSeconds = 3;
    private static readonly float targetFixedDt = 1 / 50.0f;
    private static float fixedAccumulator;
    private static float averageTimer;
    private static long frametimeSum;
    private static int numFrametimeSampled;
    private static long updateTimeSum;
    private static int numUpdateSampled;
    private static long drawTimeSum;
    private static int numDrawSampled;

    /// <summary>
    /// Gets the frametime of the previous frame in milliseconds
    /// </summary>
    public static long Frametime { get; private set; }

    /// <summary>
    /// Gets the average frametime sampled over a set period of time in milliseconds
    /// </summary>
    public static long FrametimeAvg { get; private set; }

    /// <summary>
    /// Gets the number of milliseconds taken to update one frame
    /// </summary>
    public static long UpdateTime { get; private set; }

    /// <summary>
    /// Gets the average update time sampled over a set period of time in milliseconds
    /// </summary>
    public static long UpdateTimeAvg { get; private set; }

    /// <summary>
    /// Gets the number of milliseconds taken to draw one frame
    /// </summary>
    public static long DrawTime { get; private set; }
    /// <summary>
    /// Gets the average draw time sampled over a set period of time in milliseconds
    /// </summary>
    public static long DrawTimeAvg { get; private set; }

    /// <summary>
    /// Gets time passed since last frame in seconds
    /// </summary>
    public static float DeltaTime => fps.DeltaTime;

    /// <summary>
    /// Gets current instantaneous framerate
    /// </summary>
    public static float Framerate => fps.CurrentFps;

    /// <summary>
    /// Gets the average framerate over an interval
    /// </summary>
    public static float AverageFramerate => fps.AvgFps;

    /// <summary>
    /// Gets number of times physics update function should be run
    /// </summary>
    public static int NumPhysicsUpdateToRun { get; private set; }

    /// <summary>
    /// Gets time passed since last fixed update call
    /// </summary>
    public static float FixedDeltaTime => targetFixedDt;

    /// <summary>
    /// Gets the fixed update lerp value from 0-1 to lerp between frame updates
    /// </summary>
    internal static float PhysicsLerpValue { get; private set; }

    /// <summary>
    /// Updates general performance logic
    /// </summary>
    /// <param name="gt">GameTime to grab timing information from</param>
    public static void Update(GameTime gt) {
        fps.Update(gt);

        // if timer exceeds threshold, calculate all averages and reset
        averageTimer += fps.DeltaTime;
        if (averageTimer >= averageIntervalSeconds) {
            averageTimer -= averageIntervalSeconds;

            // calc averages
            FrametimeAvg = frametimeSum / numFrametimeSampled;
            UpdateTimeAvg = updateTimeSum / numUpdateSampled;
            DrawTimeAvg = drawTimeSum / numDrawSampled;

            // reset values
            frametimeSum = 0;
            numFrametimeSampled = 0;
            updateTimeSum = 0;
            numUpdateSampled = 0;
            drawTimeSum = 0;
            numDrawSampled = 0;
        }

        NumPhysicsUpdateToRun = 0;
        fixedAccumulator += fps.DeltaTime;
        while (fixedAccumulator >= targetFixedDt) {
            fixedAccumulator -= targetFixedDt;
            NumPhysicsUpdateToRun++;
        }

        PhysicsLerpValue = fixedAccumulator / targetFixedDt;
    }

    /// <summary>
    /// Starts frametime measuring
    /// </summary>
    public static void FrametimeMeasureStart() {
        frametimeStopwatch.Restart();
    }

    /// <summary>
    /// Ends frametime measuring and saves result
    /// </summary>
    public static void FrametimeMeasureEnd() {
        frametimeStopwatch.Stop();
        Frametime = frametimeStopwatch.ElapsedMilliseconds;

        frametimeSum += Frametime;
        numFrametimeSampled++;
    }

    /// <summary>
    /// Starts draw time measuring
    /// </summary>
    public static void DrawMeasureStart() {
        drawStopwatch.Restart();
    }

    /// <summary>
    /// Ends draw time measuring and saves result
    /// </summary>
    public static void DrawMeasureEnd() {
        drawStopwatch.Stop();
        DrawTime = drawStopwatch.ElapsedMilliseconds;

        drawTimeSum += DrawTime;
        numDrawSampled++;
    }

    /// <summary>
    /// Starts update time measuring
    /// </summary>
    public static void UpdateMeasureStart() {
        updateStopwatch.Restart();
    }

    /// <summary>
    /// Ends update time measuring and saves result
    /// </summary>
    public static void UpdateMeasureEnd() {
        updateStopwatch.Stop();
        UpdateTime = updateStopwatch.ElapsedMilliseconds;

        updateTimeSum += UpdateTime;
        numUpdateSampled++;
    }
}
