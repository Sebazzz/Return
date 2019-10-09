// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AutoResettingBoolean.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Services {
    public class AutoResettingBoolean {
        private readonly bool _initialValue;
        private bool _currentValue;

        public AutoResettingBoolean(bool initialValue) {
            this._initialValue = initialValue;
            this._currentValue = initialValue;
        }

        public bool GetValue() {
            bool ret = this._currentValue;
            this._currentValue = this._initialValue;
            return ret;
        }

        public void Set() => this._currentValue = !this._initialValue;
    }
}
