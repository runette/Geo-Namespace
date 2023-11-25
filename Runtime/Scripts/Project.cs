// copyright Runette Software Ltd, 2020-23. All rights reserved
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using Virgis;
using System.Linq;

namespace Project
{
    public class GisProject : GisProjectPrototype
    {
        [JsonIgnore]
        public string path
        {
            set
            {
                foreach (RecordSet set in RecordSets)
                {
                    set.path = value;
                }
            }
        }
        protected override string TYPE  { get => "Runette";}
        protected override string VERSION { get => "2.0.0";}

        [JsonProperty(PropertyName = "recordsets", Required = Required.Always)]
        [JsonConverter(typeof(RecordsetConverter))]
        public new List<RecordSet> RecordSets;
    }

    public class RecordSet : RecordSetPrototype
    {
        [JsonIgnore]
        private string m_Path;

        [JsonIgnore]
        public string path { get { return m_Path; } set
            {
                m_Path= value;
                Properties.path = value;
                if ( Units != null ) foreach(Unit unit in Units.Values)
                {
                    unit.path = value;
                }
            } }

        [JsonProperty(PropertyName = "datatype", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public RecordSetDataType DataType;
        [JsonProperty(PropertyName = "source")]
        public string m_source;
        [JsonIgnore]
        public string Source { get { return Path.GetFullPath(Path.Combine( path, m_source)); }
            set { m_source = value; } }
        [JsonProperty(PropertyName = "properties")]
        public GeogData Properties;
        [JsonProperty(PropertyName = "proj4")]
        public string Crs;

        /// <summary>
        /// Dictionary of symbology units for this layer
        /// </summary>
        [JsonProperty(PropertyName = "units", NullValueHandling = NullValueHandling.Ignore)]
        public new Dictionary<string, Unit> Units = new();

        /// <summary>
        /// List of Data Units for this layer
        /// </summary>
        [JsonProperty(PropertyName = "data_units")]
        public new List<DataUnit> DataUnits;
    }

    public class RecordsetConverter : JsonConverter
    {
        public RecordsetConverter()
        {

        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(RecordSet).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.StartArray:
                    JArray jarray = JArray.Load(reader);
                    IList<JObject> sets = jarray.Select(c => (JObject)c).ToList();
                    List<RecordSet> result = new List<RecordSet>();
                    foreach (JObject set in sets)
                    {
                        result.Add(set.ToObject(typeof(RecordSet)) as RecordSet);
                    }
                    return result;
            }

            throw new JsonReaderException("expected null, object or array token but received " + reader.TokenType);
        }


        public override void WriteJson(JsonWriter writer, object vector, JsonSerializer serializer)
        {
            serializer.Serialize(writer, vector);
        }
    }

    /// <summary>
    /// Object that holds the layer properties
    /// </summary>
    public struct GeogData {

        [JsonIgnore]
        private string m_Path;

        [JsonIgnore]
        public string path { 
            get { return m_Path; }
            set { 
                m_Path = value;
                bhdata.path = value;
            }
        }

        /// <summary>
        /// DEM or DTM to map these values onto
        /// </summary>
        [JsonProperty(PropertyName = "dem")]
        public string m_Dem;

        [JsonIgnore]
        public string Dem { 
            get {  
                if (
                    m_Dem == null) return null;
                    return Path.GetFullPath(Path.Combine(path, m_Dem));
            } 
        }
        /// <summary>
        /// Header string to be used when converting raster bands to point cloud data for vizualisation
        /// identifies the properties names that the raster bands are mapped to in order
        /// </summary>
        [JsonProperty(PropertyName = "header-string")]
        public string headerString;
        /// <summary>
        /// PDAL Filter String
        /// </summary>
        [JsonProperty(PropertyName = "filter")]
        public List<Dictionary<string, object>> Filter;
        /// <summary>
        /// Bounding Box
        /// </summary>
        [JsonProperty(PropertyName = "bbox")]
        public List<double> BBox;
        /// <summary>
        /// GDAL Source Type
        /// </summary>
        [JsonProperty(PropertyName = "source-type", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue(SourceType.File)]
        public SourceType SourceType;
        /// <summary>
        /// Open Read only ?
        /// </summary>
        [JsonProperty(PropertyName = "read-only", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool ReadOnly;
        /// <summary>
        /// MapBox Map Scale Factor
        /// </summary>
        [JsonProperty(PropertyName = "mapscale")]
        public Int32 MapScale;
        /// <summary>
        /// MapBox Map Size FGactor
        /// </summary>
        [JsonProperty(PropertyName = "map_size")]
        public int MapSize;
        /// <summary>
        /// Mapbox Elevation Source Type
        /// </summary>
        [JsonProperty(PropertyName = "elevation_source_type", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("MapboxTerrain")]
        public string elevationSourceType;
        /// <summary>
        /// MapBox Elevation Layer4 Type 
        /// </summary>
        [JsonProperty(PropertyName = "elevation_layer_type", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("FlatTerrain")]
        public string elevationLayerType;
        /// <summary>
        /// MapBox Imagery Source Type
        /// </summary>
        [JsonProperty(PropertyName = "imagery_source_type", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("MapboxOutdoors")]
        public string imagerySourceType;
        /// <summary>
        /// Folder to use to fins XSecvt images
        /// </summary>
        [JsonProperty(PropertyName = "image_folder")]
        public string m_ImageSource;

        [JsonIgnore]
        public string imageSource {
            get
            {
                if (m_ImageSource is not null)
                {
                    return Path.GetFullPath(Path.Combine(path, m_ImageSource));
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Borehole Data Object
        /// </summary>
        [JsonProperty(PropertyName = "bh-data")]
        public BoreHoleData bhdata;

        [JsonProperty(PropertyName = "hide-sublayers")]
        public List<string> hideSublayers;
    }


    public struct BoreHoleData
    {
        [JsonIgnore]
        public string path;
        
        [JsonProperty(PropertyName = "x-field")]
        public string xField;
        [JsonProperty(PropertyName = "y-field")]
        public string yField;
        [JsonProperty(PropertyName = "z-field")]
        public string zField;
        [JsonProperty(PropertyName = "id-field")]
        public string idField;
        [JsonProperty(PropertyName = "inc-field")]
        public string incField;
        [JsonProperty(PropertyName = "azi-field")]
        public string aziField;
        [JsonProperty(PropertyName = "from-field")]
        public string fromField;
        [JsonProperty(PropertyName = "to-field")]
        public string toField;
        [JsonProperty(PropertyName = "data-field")]
        public string dataField;
        [JsonProperty(PropertyName = "data-source")]
        public string m_DataSource;

        [JsonIgnore]
        public string dataSource { get { return Path.GetFullPath(Path.Combine(path, m_DataSource)); } }
        [JsonProperty(PropertyName = "log-id-field")]
        public string logIdField;
        [JsonProperty(PropertyName = "legend")]
        [JsonConverter(typeof(LegendConverter))]
        public Dictionary<string, SerializableColor> legend;
        [JsonProperty(PropertyName = "eoh-field")]
        public string lengthField;
    }


    public class LegendConverter : JsonConverter
    {
        public LegendConverter()
        {
            // do nothing
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Dictionary<string,SerializableColor>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.StartObject:
                    JObject jobject = JObject.Load(reader);
                    Dictionary<string, SerializableColor> result = new Dictionary<string, SerializableColor>();
                    foreach (var x in jobject)
                    {
                        string key = x.Key;
                        SerializableColor value = JsonConvert.DeserializeObject<SerializableColor>(x.Value.ToString(), new VectorConverter<SerializableColor>());
                        result.Add(key, value);
                    }

                    return result;
            }

            throw new JsonReaderException("expected null, object or array token but received " + reader.TokenType);
        }


        public override void WriteJson(JsonWriter writer, object legend, JsonSerializer serializer)
        {
            Dictionary<string, object> temp = new Dictionary<string, object>();
            Dictionary<string, SerializableColor> newlegend = (legend as Dictionary<string, SerializableColor>);
            foreach (KeyValuePair<string, SerializableColor> kv in newlegend)
            {
                temp.Add(kv.Key, kv.Value.ToArray());
            }
            serializer.Serialize(writer, temp);
        }
    }

    /// <summary>
    /// Acceptable values for Recordset Type
    /// </summary>
    public enum RecordSetDataType{
        MapBox,
        Vector,
        Raster,
        PointCloud,
        Mesh,
        Mdal,
        Point,
        Line,
        Polygon,
        DEM,
        Graph,
        XSect,
        BoreHole,
        Voxel,
        Data
    }


    /// <summary>
    /// Acceptable values for the Source field of a recordset
    /// </summary>
    public enum SourceType {
        File,
        WFS,
        OAPIF,
        WMS,
        WCS,
        PG,
        AWS,
        GCS,
        Azure,
        Alibaba,
        Openstack,
        TCP,
    }



    public class Unit : UnitPrototype
    {
        [JsonIgnore]
        public string path;

        /// <summary>
        /// The transfor to be applied to the unit of symnbology
        /// </summary>

        [JsonProperty(PropertyName = "texture-image")]
        public string m_TextureImage;

        [JsonIgnore]
        public string TextureImage
        {
            get
            {
                if (m_TextureImage is not null && m_TextureImage != "")
                {
                    return Path.GetFullPath(Path.Combine(path, m_TextureImage));
                }
                else
                {
                    return null;
                }
            }
        }

    }

    public class DataUnit: DataUnitPrototype {
        /// <summary>
        /// Dictionary of symbology units for this data unit
        /// </summary>
        [JsonProperty(PropertyName = "units")]
        public new Dictionary<string, Unit> Units;
    }
}
