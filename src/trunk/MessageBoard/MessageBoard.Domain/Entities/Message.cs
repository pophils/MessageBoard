using System;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace MessageBoard.Domain.Entities
{
    [Serializable]
    public class Message : Entity<long>
    {
        public virtual long OwnerId { get; set; }
        public virtual string Content { get; set; }
        public virtual DateTime DateSaved { get; set; }
    }


    class MessageMap : ClassMapping<Message>
    {
        public MessageMap()
        {
            Table("Message");
            Id<long>(x => x.Id, mp => { mp.Column("Id"); mp.Generator(Generators.Native); });
            Property<long>(x => x.OwnerId, mp => { mp.Column("OwnerId"); });
            Property<string>(x => x.Content, mp => { mp.Column("Content"); });
            Property<DateTime>(x => x.DateSaved, mp => { mp.Column("DateSaved"); }); 
        }
    }
}

    
   
   
   
   
   
   
   
   
   