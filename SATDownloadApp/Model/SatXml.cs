using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace SATDownloadApp.Model
{
    public class SatXml
    {
        [XmlIgnore]
        public virtual Guid Id { get; set; }

        [XmlIgnore]
        public virtual Ticket RequestTicket { get; set; }

        /// <summary>
        /// TODO: Se podria usar como id, dado que es un GUID y deberia ser unico.
        /// Hay que resolver el tema de si alguien solicita la descarga de un archivo que ya existe.
        /// El servicio no deberia descargar xms que ya esten en la base.
        /// un xml puede ser solicitado mas de una vez (many-to-many!)
        /// </summary>
        public virtual string ExternalId { get; set; }

        public virtual string Document { get; set; }
        
    }
}