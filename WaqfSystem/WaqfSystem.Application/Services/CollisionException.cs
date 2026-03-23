using System;

namespace WaqfSystem.Application.Services
{
    public class CollisionException : Exception
    {
        public string? LockedBy { get; }

        public CollisionException(string message, string? lockedBy = null) : base(message)
        {
            LockedBy = lockedBy;
        }
    }
}
