namespace Nine.Animation
{
    using System;
    using static System.Math;

    /// <summary>
    /// Basic class for all timeline based animations.
    /// </summary>
    public abstract class Timeline : Animation
    {
        private double elapsedTime;

        /// <summary>
        /// Gets or set the duration (ms) for this animation. This value is not affected by BeginTime, EndTime and Repeat.
        /// </summary>
        public double Duration { get; set; } = DefaultDuration;

        public static double DefaultDuration { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the time at which this <see cref="Timeline"/> should begin.
        /// </summary>
        public double? BeginTime { get; set; }

        /// <summary>
        /// Gets or sets the time at which this <see cref="Timeline"/> should end.
        /// </summary>
        public double? EndTime { get; set; }

        /// <summary>
        /// Gets the position of the animation as an elapsed time since the begin point.
        /// Counts up if the direction is Forward, down if Backward.
        /// </summary>
        public double Position { get; private set; }

        /// <summary>
        /// Gets or sets the playing speed of this animation.
        /// </summary>
        public double Speed { get; set; } = DefaultSpeed;

        public static double DefaultSpeed { get; set; } = 1.0;

        /// <summary>
        /// Gets whether this animation should play backwards after it reaches the end.
        /// Takes effect when an animation would otherwise complete.
        /// </summary>
        public bool Yoyo { get; set; }
        
        /// <summary>
        /// Gets or set whether the animation is currently playing forward or backward.
        /// Takes effect on an animation that is playing or paused.
        /// </summary>
        public bool Forward { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of times this animation will be played.
        /// When set to a fractional value, the animation will be stopped and completed part way.
        /// Float.MaxValue means forever. The default value is 1.
        /// </summary>
        public double Repeat { get; set; } = 1;
        
        /// <summary>
        /// Occurs when this animation has just repeated.
        /// </summary>
        public event Action Repeated;

        /// <summary>
        /// Positions the animation at the specified time value between 0 and Duration.
        /// Takes effect on an animation that is playing or paused.
        /// Adjusts elapsed time, so that animation will stop on time.
        /// </summary>
        public void Seek(double position)
        {
            elapsedTime = position;
            Update(0);
        }

        /// <summary>
        /// When overridden, positions the animation at the specified percentage.
        /// </summary>
        protected abstract void Seek(double percentage, double previousPercentage);

        protected override bool UpdateCore(double dt)
        {
            var ended = false;

            var increment = dt * Speed;

            var beginPosition = Max(0, BeginTime ?? 0);

            var endPosition = Min(EndTime ?? Duration, Duration);

            var trimmedDuration = endPosition - beginPosition;

            if (trimmedDuration <= 0) return true;

            var totalDuration = Repeat * trimmedDuration;

            var previousRepeat = Min(Floor(elapsedTime / trimmedDuration), Repeat - 1);

            elapsedTime += increment;

            if (elapsedTime > totalDuration)
            {
                ended = true;
                elapsedTime = totalDuration;
            }

            var nextRepeat = Min(Floor(elapsedTime / trimmedDuration), Repeat - 1);

            var nextPosition = elapsedTime - nextRepeat * trimmedDuration;

            var isReversed = (Yoyo && nextRepeat % 2 == 1 ? Forward : !Forward);

            var previousPosition = Position;

            Position = isReversed ? endPosition - nextPosition : beginPosition + nextPosition;

            if (ended && Floor(Repeat) == Repeat && (isReversed && BeginTime == null || !isReversed && EndTime == null))
            {
                Seek(isReversed ? 0.0 : 1.0, previousPosition / Duration);
            }
            else
            {
                Seek(Position / Duration, previousPosition / Duration);
            }

            var repeated = Repeated;
            if (repeated != null)
            {
                var repeatCount = nextRepeat - previousRepeat;
                for (var i = 0; i < repeatCount; i++)
                {
                    repeated();
                }
            }

            return ended;
        }

        public override void InheritFrom(IAnimation other)
        {
            base.InheritFrom(other);

            var anim = other as Timeline;
            if (anim != null)
            {
                Duration = anim.Duration;
                Forward = anim.Forward;
                BeginTime = anim.BeginTime;
                EndTime = anim.EndTime;
                Repeat = anim.Repeat;
                Speed = anim.Speed;
                Yoyo = anim.Yoyo;
            }
        }
    }
}