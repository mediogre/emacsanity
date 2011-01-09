namespace IronElisp
{
    public class font_driver
    {
        // COMEBACK_WHEN_READY!!!
    }

    public class font_driver_list
    {
        /* 1 iff this driver is currently used.  It is igonred in the global
           font driver list.*/
        bool on;
        /* Pointer to the font driver.  */
        font_driver driver;
        /* Pointer to the next element of the chain.  */
        font_driver_list next;
    }

    public class font_data_list
    {
        /* Pointer to the font driver.  */
        font_driver driver;
        /* Data specific to the font driver.  */
        object data;
        /* Pointer to the next element of the chain.  */
        font_data_list next;
    }

    /* An enumerator for each font property.  This is used as an index to
       the vector of FONT-SPEC and FONT-ENTITY.

       Note: The order is important and should not be changed.  */
    public enum font_property_index
    {
        /* FONT-TYPE is a symbol indicating a font backend; currently `x',
           `xft', `ftx' are available on X, `uniscribe' and `gdi' on
           Windows, and `ns' under Cocoa / GNUstep.  */
        FONT_TYPE_INDEX,

        /* FONT-FOUNDRY is a foundry name (symbol).  */
        FONT_FOUNDRY_INDEX,

        /* FONT-FAMILY is a family name (symbol).  */
        FONT_FAMILY_INDEX,

        /* FONT-ADSTYLE is an additional style name (symbol).  */
        FONT_ADSTYLE_INDEX,

        /* FONT-REGISTRY is a combination of a charset-registry and
           charset-encoding name (symbol).  */
        FONT_REGISTRY_INDEX,

        /* FONT-WEIGHT is a numeric value of weight (e.g. medium, bold) of
           the font.  The lowest 8-bit is an index determining the
           symbolic name, and the higher bits is the actual numeric value
           defined in `font-weight-table'. */
        FONT_WEIGHT_INDEX,

        /* FONT-SLANT is a numeric value of slant (e.g. r, i, o) of the
           font.  The lowest 8-bit is an index determining the symbolic
           name, and the higher bits is the actual numeric value defined
           in `font-slant-table'.  */
        FONT_SLANT_INDEX,

        /* FONT-WIDTH is a numeric value of setwidth (e.g. normal) of the
           font.  The lowest 8-bit is an index determining the symbolic
           name, and the higher bits is the actual numeric value defined
           `font-width-table'.  */
        FONT_WIDTH_INDEX,

        /* FONT-SIZE is a size of the font.  If integer, it is a pixel
           size.  For a font-spec, the value can be float specifying a
           point size.  The value zero means that the font is
           scalable.  */
        FONT_SIZE_INDEX,

        /* FONT-DPI is a resolution (dot per inch) for which the font is
           designed. */
        FONT_DPI_INDEX,

        /* FONT-SPACING is a spacing (mono, proportional, charcell) of the
           font (integer; one of enum font_spacing).  */
        FONT_SPACING_INDEX,

        /* FONT-AVGWIDTH is an average width (1/10 pixel unit) of the
           font.  */
        FONT_AVGWIDTH_INDEX,

        /* In a font-spec, the value is an alist of extra information of a
           font such as name, OpenType features, and language coverage.
           In addition, in a font-entity, the value may contain a pair
           (font-entity . INFO) where INFO is extra information to identify
           a font (font-driver dependent).  */
        FONT_EXTRA_INDEX,		/* alist		alist */

        /* This value is the length of font-spec vector.  */
        FONT_SPEC_MAX,

        /* The followings are used only for a font-entity.  */

        /* List of font-objects opened from the font-entity.  */
        FONT_OBJLIST_INDEX = FONT_SPEC_MAX,

        /* This value is the length of font-entity vector.  */
        FONT_ENTITY_MAX,

        /* XLFD name of the font (string). */
        FONT_NAME_INDEX = FONT_ENTITY_MAX,

        /* Full name of the font (string).  It is the name extracted from
           the opend font, and may be different from the above.  It may be
           nil if the opened font doesn't give a name.  */
        FONT_FULLNAME_INDEX,

        /* File name of the font or nil if a file associated with the font
           is not available.  */
        FONT_FILE_INDEX,

        /* Format of the font (symbol) or nil if unknown.  */
        FONT_FORMAT_INDEX,

        /* This value is the length of font-object vector.  */
        FONT_OBJECT_MAX
    }

    public abstract class LispFontish : LispObject, LispVectorLike<LispObject>
    {
        protected LispVector v_;

        public int Size
        {
            get
            {
                return v_.Size;
            }
        }

        public LispObject this[int index]
        {
            get
            {
                return v_[index];
            }

            set
            {
                v_[index] = value;
            }
        }
    }

    public class FontSpec : LispFontish
    {
        public FontSpec()
        {
            v_ = new LispVector((int) font_property_index.FONT_SPEC_MAX);
        }
    }

    public class FontEntity : LispFontish
    {
        public FontEntity()
        {
            v_ = new LispVector((int)font_property_index.FONT_ENTITY_MAX);
        }
    }

    public class Font : LispFontish
    {
        // Lisp_Object props[FONT_OBJECT_MAX];

        /* Beyond here, there should be no more Lisp_Object components.  */

        /* Maximum bound width over all existing characters of the font.  On
           X window, this is same as (font->max_bounds.width).  */
        int max_width;

        /* By which pixel size the font is opened.  */
        int pixel_size;

        /* Height of the font.  On X window, this is the same as
           (font->ascent + font->descent).  */
        int height;

        /* Width of the space glyph of the font.  If the font doesn't have a
           SPACE glyph, the value is 0.  */
        int space_width;

        /* Average width of glyphs in the font.  If the font itself doesn't
           have that information but has glyphs of ASCII character, the
           value is the average with of those glyphs.  Otherwise, the value
           is 0.  */
        int average_width;

        /* Minimum glyph width (in pixels).  */
        int min_width;

        /* Ascent and descent of the font (in pixels).  */
        int ascent, descent;

        /* Vertical pixel width of the underline.  If is zero if that
           information is not in the font.  */
        int underline_thickness;

        /* Vertical pixel position (relative to the baseline) of the
           underline.  If it is positive, it is below the baseline.  It is
           negative if that information is not in the font.  */
        int underline_position;

        /* 1 if `vertical-centering-font-regexp' matches this font name.
           In this case, we render characters at vartical center positions
           of lines.  */
        int vertical_centering;

        /* Encoding type of the font.  The value is one of
           0, 1, 2, or 3:
          0: code points 0x20..0x7F or 0x2020..0x7F7F are used
          1: code points 0xA0..0xFF or 0xA0A0..0xFFFF are used
          2: code points 0x20A0..0x7FFF are used
          3: code points 0xA020..0xFF7F are used
           If the member `font_encoder' is not NULL, this member is ignored.  */
        byte encoding_type;

        /* The baseline position of a font is normally `ascent' value of the
           font.  However, there exists many fonts which don't set `ascent'
           an appropriate value to be used as baseline position.  This is
           typical in such ASCII fonts which are designed to be used with
           Chinese, Japanese, Korean characters.  When we use mixture of
           such fonts and normal fonts (having correct `ascent' value), a
           display line gets very ugly.  Since we have no way to fix it
           automatically, it is users responsibility to supply well designed
           fonts or correct `ascent' value of fonts.  But, the latter
           requires heavy work (modifying all bitmap data in BDF files).
           So, Emacs accepts a private font property
           `_MULE_BASELINE_OFFSET'.  If a font has this property, we
           calculate the baseline position by subtracting the value from
           `ascent'.  In other words, the value indicates how many bits
           higher we should draw a character of the font than normal ASCII
           text for a better looking.

           We also have to consider the fact that the concept of `baseline'
           differs among scripts to which each character belongs.  For
           instance, baseline should be at the bottom most position of all
           glyphs for Chinese, Japanese, and Korean.  But, many of existing
           fonts for those characters doesn't have correct `ascent' values
           because they are designed to be used with ASCII fonts.  To
           display characters of different language on the same line, the
           best way will be to arrange them in the middle of the line.  So,
           in such a case, again, we utilize the font property
           `_MULE_BASELINE_OFFSET'.  If the value is larger than `ascent' we
           calculate baseline so that a character is arranged in the middle
           of a line.  */
        int baseline_offset;

        /* Non zero means a character should be composed at a position
           relative to the height (or depth) of previous glyphs in the
           following cases:
          (1) The bottom of the character is higher than this value.  In
          this case, the character is drawn above the previous glyphs.
          (2) The top of the character is lower than 0 (i.e. baseline
          height).  In this case, the character is drawn beneath the
          previous glyphs.

           This value is taken from a private font property
           `_MULE_RELATIVE_COMPOSE' which is introduced by Emacs.  */
        int relative_compose;

        /* Non zero means an ascent value to be used for a character
           registered in char-table `use-default-ascent'.  */
        int default_ascent;

        /* CCL program to calculate code points of the font.  */
        ccl_program font_encoder;

        /* Font-driver for the font.  */
        font_driver driver;

        /* Charset to encode a character code into a glyph code of the font.
           -1 means that the font doesn't require this information to encode
           a character.  */
        int encoding_charset;

        /* Charset to check if a character code is supported by the font.
           -1 means that the contents of the font must be looked up to
           determine it.  */
        int repertory_charset;

        /* There will be more to this structure, but they are private to a
           font-driver.  */

        public Font(FontEntity entity, int pixel_size)
        {
            v_ = new LispVector((int)font_property_index.FONT_OBJECT_MAX);

            if (!L.NILP(entity))
            {
                for (int i = 1; i < (int) font_property_index.FONT_SPEC_MAX; i++)
                    this[i] = L.AREF(entity, i);

                if (!L.NILP(L.AREF(entity, (int) font_property_index.FONT_EXTRA_INDEX)))
                    this[(int)font_property_index.FONT_EXTRA_INDEX]
                      = F.copy_sequence(L.AREF(entity, (int)font_property_index.FONT_EXTRA_INDEX));
            }

            this[(int) font_property_index.FONT_SIZE_INDEX] = L.make_number(pixel_size);
        }
    }

    public partial class L
    {
        /* Predicates to check various font-related objects.  */
        /* 1 iff X is one of font-spec, font-entity, and font-object.  */
        public static bool FONTP(LispObject x)
        {
            return x is LispFontish;
        }
        /* 1 iff X is font-spec.  */
        public static bool FONT_SPEC_P(LispObject x)
        {
            return x is FontSpec;
        }
        /* 1 iff X is font-entity.  */
        public static bool FONT_ENTITY_P(LispObject x)
        {
            return x is FontEntity;
        }
        /* 1 iff X is font-object.  */
        public static bool FONT_OBJECT_P(LispObject x)
        {
            return x is Font;
        }
    }
}