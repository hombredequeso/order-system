
// Taken from:
// https://github.com/mikhailshilkov/mikhailio-samples/blob/master/Either%7BTL%2CTR%7D.cs

using System.Collections.Generic;

namespace Hdq.Lib
{
    using System;


    public static class EitherExtensions
    {
        public static Either<TL, TR2> Bind<TL, TR1, TR2>(this Either<TL, TR1> eIn, Func<TR1, Either<TL, TR2>> f)
        {
            return eIn.Match(
                l => l,
                r1 => f(r1)
            );
        }

    }

    /// <summary>
    /// Functional data data to represent a discriminated
    /// union of two possible types.
    /// </summary>
    /// <typeparam name="TL">Type of "Left" item.</typeparam>
    /// <typeparam name="TR">Type of "Right" item.</typeparam>
    public class Either<TL, TR> : IEquatable<Either<TL, TR>>
    {
        public bool Equals(Either<TL, TR> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TL>.Default.Equals(left, other.left) && EqualityComparer<TR>.Default.Equals(right, other.right) && isLeft == other.isLeft;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Either<TL, TR>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<TL>.Default.GetHashCode(left);
                hashCode = (hashCode * 397) ^ EqualityComparer<TR>.Default.GetHashCode(right);
                hashCode = (hashCode * 397) ^ isLeft.GetHashCode();
                return hashCode;
            }
        }

        private readonly TL left;
        private readonly TR right;
        private readonly bool isLeft;

        public Either(TL left)
        {
            this.left = left;
            this.isLeft = true;
        }

        public Either(TR right)
        {
            this.right = right;
            this.isLeft = false;
        }

        public T Match<T>(Func<TL, T> leftFunc, Func<TR, T> rightFunc)
        {
            if (leftFunc == null)
            {
                throw new ArgumentNullException(nameof(leftFunc));
            }

            if (rightFunc == null)
            {
                throw new ArgumentNullException(nameof(rightFunc));
            }

            return this.isLeft ? leftFunc(this.left) : rightFunc(this.right);
        }

        public void Do(Action<TL> leftAction, Action<TR> rightAction)
        {
            if (leftAction == null)
            {
                throw new ArgumentNullException(nameof(leftAction));
            }

            if (rightAction == null)
            {
                throw new ArgumentNullException(nameof(rightAction));
            }
            if (this.isLeft)
                leftAction(this.left);
            else
            {
                rightAction(this.right);
            }
        }
        /// <summary>
        /// If right value is assigned, execute an action on it.
        /// </summary>
        /// <param name="rightAction">Action to execute.</param>
        public void DoRight(Action<TR> rightAction)
        {
            if (rightAction == null)
            {
                throw new ArgumentNullException(nameof(rightAction));
            }

            if (!this.isLeft)
            {                
                rightAction(this.right);
            }
        }

        public TL LeftOrDefault() => this.Match(l => l, r => default(TL));

        public TR RightOrDefault() => this.Match(l => default(TR), r => r);

        public static implicit operator Either<TL, TR>(TL left) => new Either<TL, TR>(left);

        public static implicit operator Either<TL, TR>(TR right) => new Either<TL, TR>(right);
    }
}
