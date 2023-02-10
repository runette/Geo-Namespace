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
        private const string TYPE = "geo";
        private const string VERSION = "1.1.0";

        public static string GetVersion()
        {
            return $"{TYPE}:{VERSION}";
        }

        [JsonProperty(PropertyName = "recordsets", Required = Required.Always)]
        [JsonConverter(typeof(RecordsetConverter))]
        public List<RecordSet> RecordSets;
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
                foreach(Unit unit in Units.Values)
                {
                    unit.path = value;
                }
            }
        }
        /// <summary>
        /// list of symbology units for this layer
        /// </summary>
        [JsonProperty(PropertyName = "units")]
        public Dictionary<string, Unit> Units;
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
        /// Color mode to be used for raster layers
        /// </summary>
        [JsonProperty(PropertyName = "color-mode", DefaultValueHandling = DefaultValueHandling.Populate)]
        [JsonConverter(typeof(StringEnumConverter))]
        [DefaultValue("SinglebandGrey")]
        public ColorMode ColorMode;
        /// <summary>
        /// PDAL Colorinterp strnig
        /// </summary>
        [JsonProperty(PropertyName = "colorinterp")]
        public Dictionary<string, object> ColorInterp;
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
        Voxel
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

    /// <summary>
    /// Acceptable value for the Shape field in Symbology
    /// </summary>
    public enum Shapes
    {
        Spheroid,
        Cuboid,
        Cylinder
    }

    /// <summary>
    /// Acceptable values for color-mode
    /// </summary>
    public enum ColorMode
    {
        MultibandColor,
        SinglebandColor,
        SinglebandGrey
    }

    public class Unit : UnitPrototype
    {
        [JsonIgnore]
        public string path;
        /// <summary>
        /// The shape to be used by the unit of symbology.
        /// 
        /// Must contain an instance of Shapes
        /// </summary>
        [JsonProperty(PropertyName = "shape", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Shapes Shape;
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
}
