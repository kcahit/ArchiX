// File: src/ArchiX.WebApplication/Mapping/ApplicationProfile.cs
using AutoMapper;

namespace ArchiX.WebApplication.Mapping
{
    /// <summary>
    /// 7,0300 — Mapping Profilleri (AutoMapper).
    /// </summary>
    public sealed class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            // Somut eşlemeleri burada tanımlayacağız.
            // Örnek:
            // CreateMap<Domain.Customer, Dto.CustomerDto>().ReverseMap();
        }
    }
}
