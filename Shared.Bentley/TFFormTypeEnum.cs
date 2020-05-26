namespace Shared.Bentley
{

public enum TFFormTypeEnum
{
    TF_FREE_FORM_ELM   = 31,
    TF_LINEAR_FORM_ELM = 32,
    TF_SLAB_FORM_ELM   = 42,
    TF_SMOOTH_FORM_ELM = 45

    // #define TF_TOP_SHAPES                        1
    // #define TF_TOP_PLANE                         2
    // #define TF_TOP_FIXED_HEIGHT                  3
    //
    // /* the different types of TriForma elements */
    // #define TF_NO_TF_ELM                         0
    // #define TF_OLD_TYPE_EXTRUSION_FORM         999
    // #define TF_FREE_FORM_ELM                    31  /* a TF free form */
    // #define TF_LINEAR_FORM_ELM                  32  /* a TF linear form */
    // #define TF_LINEAR_ELM                       33  /* a MicroStation line, linestring, arc, curve or complex linestring */
    // #define TF_SHAPE_ELM                        34  /* a MicroStation shape, ellipse or complex shape */
    // #define TF_CELL_ELM                         35  /* a MicroStation cell or shared cell instance with a TF partname */
    // #define TF_COMPOUND_CELL_ELM                36  /* a TF compound cell */
    // #define TF_ROOM_SHAPE_ELM                   37  /* a TF shape belonging to a room */
    // #define TF_ROOM_ELM                         38  /* a TF room element */
    // #define TF_LINE_STRING_FORM_ELM             39  /* a TF linestring form */
    // #define TF_SURFACE_ELM                      40  /* a MicroStation surface element, bspline surface, cone or solid element */
    // #define TF_MS_CELL_ELM                      41  /* a MicroStation cell or shared cell instance without a TF partname */
    // #define TF_SLAB_FORM_ELM                    42  /* a TF slab form */
    // #define TF_BLOB_FORM_ELM                    43  /* a TF blob form */
    // #define TF_ARC_FORM_ELM                     44  /* a TF Arc form */
    // #define TF_SMOOTH_FORM_ELM                  45  /* a TF Smooth Free Form form */
    // #define TF_PATH_FORM_ELM                    46  /* a TF path form */
    // #define TF_MORPH_FORM_ELM                   47  /* a TF Morph form */
    // #define TF_COMPOUND_FORM_ELM                48  /* a TF compound form */
    // #define TF_PART_TAG_ELM                     50  /* a MS tag with part info attached */
    // 
    // #define TF_CC_INT_DOOR_ELM                  51  /* a TF internal door (compound cell) */
    // #define TF_CC_EXT_DOOR_ELM                  52  /* a TF external door (compound cell) */
    // #define TF_CC_INT_WINDOW_ELM                53  /* a TF internal window (compound cell) */
    // #define TF_CC_EXT_WINDOW_ELM                54  /* a TF external window (compound cell) */
    // #define TF_CC_INT_DOOR_AND_WINDOW_ELM       55  /* a TF internal door and window (compound cell) */
    // #define TF_CC_EXT_DOOR_AND_WINDOW_ELM       56  /* a TF external door and window (compound cell) */
    // #define TF_CC_STAIR_ELM                     57  /* a TF stair (compound cell) */
    // #define TF_CC_GRID_ELM                      58  /* a TF grid (compound cell) */
    // #define TF_ADFCELL_ELM                      59  /* a persistent triforma ADF model (subtypes: TF_PAZCELL_ELM, TF_RFACELL_ELM) */
    // 
    // #define TF_EBREP_ELM                        60    /* embedded breps */
    // #define TF_STRUCTURAL_ELM                   61  /* a TF structural (based on free form) */
    // #define TF_MESH_ELM                         62  /* a TF Mesh Element */
    // #define TF_STRUCTSMOOTH_ELM                 63  /* a TF structural (based on smooth form) */
    // #define TF_STRUCTPATH_ELM                   64  /* a TF structural (based on path form) */
    // #define TF_STRUCTTAPER_ELM                  65
    // #define TF_CEILING_ELM                      66  /* a TF ceiling */
    // #define TF_MECHANICAL_ELM                   67  /* a TF mechanical */
    // #define TF_FEATURE_SOLID_ELM                68  /* a MicroStation Feature Solid */
    // #define TF_STAIR_ELM                        69  /* a TriForma Stair Cell */
    // #define TF_FLIGHT_ELM                       70  /* a TriForma Stair Flight Cell */
    // #define TF_TREAD_ELM                        71  /* a TriForma Stair Tread Cell */
    // #define TF_RISER_ELM                        72  /* a TriForma Stair Riser Cell */
    // #define TF_LANDING_ELM                      73  /* a TriForma Stair Landing Cell */
    // #define TF_STRINGER_ELM                     74  /* a TriForma Stair Stringer Cell */
    // #define TF_STAIRANNOTATION_ELM              75  /* a TriForma Stair Annotation Cell */
    // #define TF_SPACE_ELM                        76  /* a Space */
    // #define TF_RAILING_ELM                      77  /* a Railing */
    // #define TF_HORIZONTALRAIL_ELM               78  /* a GuardRail (member of Railing)*/
    // #define TF_POST_ELM                         79  /* a Post (member of Railing) */
    // #define TF_BALUSTER_ELM                     80  /* a Baluster (member of Railing) */
    // #define TF_SHARED_COMPOUND_CELL_ELM         81  // SharedFrameHandler
    // #define TF_SHARED_ADFCELL_ELM               82  // SharedAdfCellHandler
    // #define TF_ROOF_ELM                         83  /* a TriForma Roof Cell */
    // #define TF_RAILINGENDS_ELM                  84  /* a Baluster (member of Railing) */
    // #define TF_GRID_SYSTEM_ELM                  85  // TFColumnGridHandler
    // #define TF_RFACELL_ELM                      86  // PA-Cell with label subtype RFACELL_ELEMENT
    // #define TF_PAZCELL_ELM                      87  // traditional PA-Cell
    // 
    // #define NUM_QUANTIFIED_TYPES                88  /* last # type of TriForma element + 1 */
}

}
