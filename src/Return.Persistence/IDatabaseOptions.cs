// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IDatabaseOptions.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence {
    public interface IDatabaseOptions {
        string CreateConnectionString();
    }
}
