// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MappingTestBase.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Support {
    using Application.Common.Mapping;
    using AutoMapper;

    public class MappingTestBase {
        public MappingTestBase() {
            this.ConfigurationProvider =
                new MapperConfiguration(configure: cfg => { cfg.AddProfile<MappingProfile>(); });

            this.Mapper = this.ConfigurationProvider.CreateMapper();
        }

        public IConfigurationProvider ConfigurationProvider { get; }

        public IMapper Mapper { get; }
    }
}
