// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NotFoundException.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common {
    using System;

    public class NotFoundException : Exception {
        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.") {
        }

        public NotFoundException() {
        }

        public NotFoundException(string message) : base(message) {
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
