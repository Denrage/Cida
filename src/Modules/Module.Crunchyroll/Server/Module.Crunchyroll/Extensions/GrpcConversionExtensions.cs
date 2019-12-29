using System;
using CR = Crunchyroll;

namespace Module.Crunchyroll.Extensions
{
    public static class GrpcConversionExtensions
    {
        public static CR.SearchResponse.Types.SearchItem ToGrpc(this Datum date)
        {
            return new CR.SearchResponse.Types.SearchItem()
            {
                Id =  date.Id,
                Link = date.Link,
                Name = date.Name,
                ImageUrl = date.Img.OriginalString,
                Type = date.Type.ToGrpc()
            };
        }

        public static CR.SearchItemType ToGrpc(this TypeEnum type)
        {
            switch (type)
            {
                case TypeEnum.Allgemein:
                    return CR.SearchItemType.General;
                case TypeEnum.Manga:
                    return CR.SearchItemType.Manga;
                case TypeEnum.Person:
                    return CR.SearchItemType.Person;
                case TypeEnum.Serie:
                    return CR.SearchItemType.Series;
                case TypeEnum.Zusammengefasst:
                    return CR.SearchItemType.Summarized;
                case TypeEnum.Unknown:
                    return CR.SearchItemType.Unknown;
                    
            }
            
            throw new NotImplementedException();
        }
    }
}