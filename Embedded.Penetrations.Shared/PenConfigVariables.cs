using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Embedded.Penetrations.Shared
{
    public class PenConfigVariables : ConfigVariable        
    {
        private static List<ConfigVariable> variables_ = new List<ConfigVariable>();
      
        public static readonly PenConfigVariables
            Log = new PenConfigVariables("LOG"),
            LogFolder = new PenConfigVariables("LOG_FOLDER"),

            SqlServer = new PenConfigVariables("AEP_SAVRD_SERVER", "vibe1.sp.spbaep.ru", usePrefix: false),
            SqlPassServer = new PenConfigVariables("AEP_SAVRD_PASS_SERVER", "pw-srv.sp.spbaep.ru", usePrefix: false),
            Database = new PenConfigVariables("AEP_SAVRD_BASE", "parts", usePrefix: false),
            ProjectId = new PenConfigVariables("EMBDB_PROJECT_ID", "0", usePrefix: false),

            CellName = new PenConfigVariables("CELL_NAME", "Penetration"),
            CellNameOld = new PenConfigVariables("CELL_NAME_OLD", "EmbeddedPart"),
            DataGroupCatalogType = new PenConfigVariables("DG_CATALOG_TYPE", "EmbeddedPart"),
            DataGroupCatalogInstance = new PenConfigVariables("DG_CATALOG_INSTANCE", "Embedded Part"),
            DataGroupSchemaName = new PenConfigVariables("DG_SCHEMA_NAME", "EmbPart"),

            Level =       new PenConfigVariables("LEVEL", "C-EMBP-PNTR"),
            PerfoLevel =  new PenConfigVariables("LEVEL_PERFORATOR", "C-EMB-ANNO"),
            ProjectionLevel = new PenConfigVariables("LEVEL_PROJECTION", "C-EMB-ANNO"),
            ProjectionFlangeLevel = new PenConfigVariables("LEVEL_PROJECTION_FLANGE", "C-EMB-FLANGE"),
            ProjectionPointLevel = new PenConfigVariables("LEVEL_PROJECTION_POINT", "C-EMB-POINT");

        public PenConfigVariables(string name, string defaultValue = null, bool usePrefix = true) 
            : base((usePrefix ? prefix_ : String.Empty) + name, defaultValue) 
        {            
            variables_.Add(this);
        }

        public static List<ConfigVariable> GetVariables() => new List<ConfigVariable>(variables_);

        private const string prefix_ = "AEP_EMB_PEN_";
    }
}
