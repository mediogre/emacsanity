namespace IronElisp
{
    public enum Objects
    {
        // alloc.c
        gc_cons_percentage,
        purify_flag,
        post_gc_hook,
        memory_signal_data, 
        memory_full,
        gc_elapsed,

        // buffer.c
        /*
           4970:  DEFVAR_LISP_NOPRO ("default-mode-line-format",
           4975:  DEFVAR_LISP_NOPRO ("default-header-line-format",
           4980:  DEFVAR_LISP_NOPRO ("default-cursor-type", &buffer_defaults.cursor_type,
           4984:  DEFVAR_LISP_NOPRO ("default-line-spacing",
           4989:  DEFVAR_LISP_NOPRO ("default-cursor-in-non-selected-windows",
           4994:  DEFVAR_LISP_NOPRO ("default-abbrev-mode",
           4999:  DEFVAR_LISP_NOPRO ("default-ctl-arrow",
           5004:  DEFVAR_LISP_NOPRO ("default-direction-reversed",
           5009:  DEFVAR_LISP_NOPRO ("default-enable-multibyte-characters",
           5014:  DEFVAR_LISP_NOPRO ("default-buffer-file-coding-system",
           5019:  DEFVAR_LISP_NOPRO ("default-truncate-lines",
           5024:  DEFVAR_LISP_NOPRO ("default-fill-column",
           5029:  DEFVAR_LISP_NOPRO ("default-left-margin",
           5034:  DEFVAR_LISP_NOPRO ("default-tab-width",
           5039:  DEFVAR_LISP_NOPRO ("default-case-fold-search",
           5045:  DEFVAR_LISP_NOPRO ("default-buffer-file-type",
           5052:  DEFVAR_LISP_NOPRO ("default-left-margin-width",
           5057:  DEFVAR_LISP_NOPRO ("default-right-margin-width",
           5062:  DEFVAR_LISP_NOPRO ("default-left-fringe-width",
           5067:  DEFVAR_LISP_NOPRO ("default-right-fringe-width",
           5072:  DEFVAR_LISP_NOPRO ("default-fringes-outside-margins",
           5077:  DEFVAR_LISP_NOPRO ("default-scroll-bar-width",
           5082:  DEFVAR_LISP_NOPRO ("default-vertical-scroll-bar",
           5087:  DEFVAR_LISP_NOPRO ("default-indicate-empty-lines",
           5092:  DEFVAR_LISP_NOPRO ("default-indicate-buffer-boundaries",
           5097:  DEFVAR_LISP_NOPRO ("default-fringe-indicator-alist",
           5102:  DEFVAR_LISP_NOPRO ("default-fringe-cursor-alist",
           5107:  DEFVAR_LISP_NOPRO ("default-scroll-up-aggressively",
           5113:  DEFVAR_LISP_NOPRO ("default-scroll-down-aggressively",
           5180:  DEFVAR_LISP_NOPRO ("default-major-mode", &buffer_defaults.major_mode,
           5533:/*DEFVAR_LISP ("debug-check-symbol", &Vcheck_symbol,*/
        before_change_functions,
        after_change_functions,
        first_change_hook,
/*           5692:  DEFVAR_LISP ("transient-mark-mode", &Vtransient_mark_mode,*/

        inhibit_read_only,
/*           5737:  DEFVAR_LISP ("kill-buffer-query-functions", &Vkill_buffer_query_functions,
           5743:  DEFVAR_LISP ("change-major-mode-hook", &Vchange_major_mode_hook,
        1 match for "defvar_lisp" in buffer: bytecode.c
           1688:  DEFVAR_LISP ("byte-code-meter", &Vbyte_code_meter,
        5 matches for "defvar_lisp" in buffer: callint.c
            950:  DEFVAR_LISP ("current-prefix-arg", &Vcurrent_prefix_arg,
            958:  DEFVAR_LISP ("command-history", &Vcommand_history,
            966:  DEFVAR_LISP ("command-debug-status", &Vcommand_debug_status,
            972:  DEFVAR_LISP ("mark-even-if-inactive", &Vmark_even_if_inactive,
            980:  DEFVAR_LISP ("mouse-leave-buffer-hook", &Vmouse_leave_buffer_hook,
        10 matches for "defvar_lisp" in buffer: callproc.c
           1372:  DEFVAR_LISP ("shell-file-name", &Vshell_file_name,
           1377:  DEFVAR_LISP ("exec-path", &Vexec_path,
           1381:  DEFVAR_LISP ("exec-suffixes", &Vexec_suffixes,
           1386:  DEFVAR_LISP ("exec-directory", &Vexec_directory,
           1391:  DEFVAR_LISP ("data-directory", &Vdata_directory,
           1395:  DEFVAR_LISP ("doc-directory", &Vdoc_directory,
           1399:  DEFVAR_LISP ("configure-info-directory", &Vconfigure_info_directory,
           1406:  DEFVAR_LISP ("shared-game-score-directory", &Vshared_game_score_directory,
           1415:  DEFVAR_LISP ("initial-environment", &Vinitial_environment,
           1421:  DEFVAR_LISP ("process-environment", &Vprocess_environment, */
        // category.c
        word_combining_categories,
        word_separating_categories,
/*        3 matches for "defvar_lisp" in buffer: ccl.c
           2343:  DEFVAR_LISP ("code-conversion-map-vector", &Vcode_conversion_map_vector,
           2347:  DEFVAR_LISP ("font-ccl-encoder-alist", &Vfont_ccl_encoder_alist,
           2360:  DEFVAR_LISP ("translation-hash-table-vector", &Vtranslation_hash_table_vector, */
//        8 matches for "defvar_lisp" in buffer: character.c
        translation_table_vector,
        auto_fill_chars,
        char_width_table,
        char_direction_table,
        printable_chars,
        char_script_table,
        script_representative_chars,
        unicode_category_table,
                               /*        3 matches for "defvar_lisp" in buffer: charset.c
           2389:  DEFVAR_LISP ("charset-map-path", &Vcharset_map_path,
           2397:  DEFVAR_LISP ("charset-list", &Vcharset_list,
           2401:  DEFVAR_LISP ("current-iso639-language", &Vcurrent_iso639_language,
        3 matches for "defvar_lisp" in buffer: cmds.c
            581:  DEFVAR_LISP ("self-insert-face", &Vself_insert_face,
            586:  DEFVAR_LISP ("self-insert-face-command", &Vself_insert_face_command,
            591:  DEFVAR_LISP ("blink-paren-function", &Vblink_paren_function,
        23 matches for "defvar_lisp" in buffer: coding.c
          10544:  DEFVAR_LISP ("coding-system-list", &Vcoding_system_list,
          10552:  DEFVAR_LISP ("coding-system-alist", &Vcoding_system_alist,
          10562:  DEFVAR_LISP ("coding-category-list", &Vcoding_category_list,
          10581:  DEFVAR_LISP ("coding-system-for-read", &Vcoding_system_for_read,
          10590:  DEFVAR_LISP ("coding-system-for-write", &Vcoding_system_for_write,
          10604:  DEFVAR_LISP ("last-coding-system-used", &Vlast_coding_system_used,
          10609:  DEFVAR_LISP ("last-code-conversion-error", &Vlast_code_conversion_error,
          10640:  DEFVAR_LISP ("file-coding-system-alist", &Vfile_coding_system_alist,
          10661:  DEFVAR_LISP ("process-coding-system-alist", &Vprocess_coding_system_alist,
          10677:  DEFVAR_LISP ("network-coding-system-alist", &Vnetwork_coding_system_alist,
          10694:  DEFVAR_LISP ("locale-coding-system", &Vlocale_coding_system,
          10700:  DEFVAR_LISP ("eol-mnemonic-unix", &eol_mnemonic_unix,
          10705:  DEFVAR_LISP ("eol-mnemonic-dos", &eol_mnemonic_dos,
          10710:  DEFVAR_LISP ("eol-mnemonic-mac", &eol_mnemonic_mac,
          10715:  DEFVAR_LISP ("eol-mnemonic-undecided", &eol_mnemonic_undecided,
          10720:  DEFVAR_LISP ("enable-character-translation", &Venable_character_translation,
          10725:  DEFVAR_LISP ("standard-translation-table-for-decode",
          10730:  DEFVAR_LISP ("standard-translation-table-for-encode",
          10735:  DEFVAR_LISP ("charset-revision-table", &Vcharset_revision_table,
          10742:  DEFVAR_LISP ("default-process-coding-system",
          10749:  DEFVAR_LISP ("latin-extra-code-table", &Vlatin_extra_code_table,
          10761:  DEFVAR_LISP ("select-safe-coding-system-function",
          10825:  DEFVAR_LISP ("translation-table-for-input", &Vtranslation_table_for_input, */
        // composite.c
/*           1762:  DEFVAR_LISP ("compose-chars-after-function", &Vcompose_chars_after_function, */
        auto_composition_function,
        composition_function_table,
/*        2 matches for "defvar_lisp" in buffer: data.c
           3195:  DEFVAR_LISP ("most-positive-fixnum", &Vmost_positive_fixnum,
           3200:  DEFVAR_LISP ("most-negative-fixnum", &Vmost_negative_fixnum,
        2 matches for "defvar_lisp" in buffer: dbusbind.c
           1884:  DEFVAR_LISP ("dbus-registered-functions-table",
           1915:  DEFVAR_LISP ("dbus-debug", &Vdbus_debug,
        1 match for "defvar_lisp" in buffer: dired.c
           1066:  DEFVAR_LISP ("completion-ignored-extensions", &Vcompletion_ignored_extensions, */
        // dispnew.c
/*           6271:  DEFVAR_LISP ("initial-window-system", &Vinitial_window_system,
           6281:  DEFVAR_LISP ("window-system-version", &Vwindow_system_version,
           6288:  DEFVAR_LISP ("glyph-table", &Vglyph_table,*/
        standard_display_table,
/*           6309:  DEFVAR_LISP ("redisplay-preemption-period", &Vredisplay_preemption_period,
*/
         // doc.c
            doc_file_name,
            build_files,
/*
        3 matches for "defvar_lisp" in buffer: dosfns.c
            727:  DEFVAR_LISP ("dos-version", &Vdos_version,
            731:  DEFVAR_LISP ("dos-windows-version", &Vdos_windows_version,
            735:  DEFVAR_LISP ("dos-display-scancodes", &Vdos_display_scancodes,
*/
            // editfns.c
            inhibit_field_text_motion,
            buffer_access_fontify_functions,
            buffer_access_fontified_property,
/*           4600:  DEFVAR_LISP ("system-name", &Vsystem_name,
           4603:  DEFVAR_LISP ("user-full-name", &Vuser_full_name,
           4606:  DEFVAR_LISP ("user-login-name", &Vuser_login_name,
           4609:  DEFVAR_LISP ("user-real-login-name", &Vuser_real_login_name,
           4612:  DEFVAR_LISP ("operating-system-release", &Voperating_system_release,
        15 matches for "defvar_lisp" in buffer: emacs.c
           1517:  DEFVAR_LISP ("command-line-args", &Vcommand_line_args,
           1521:  DEFVAR_LISP ("system-type", &Vsystem_type,
           1533:  DEFVAR_LISP ("system-configuration", &Vsystem_configuration,
           1539:  DEFVAR_LISP ("system-configuration-options", &Vsystem_configuration_options,
           1546:  DEFVAR_LISP ("kill-emacs-hook", &Vkill_emacs_hook,
           1565:  DEFVAR_LISP ("path-separator", &Vpath_separator,
           1573:  DEFVAR_LISP ("invocation-name", &Vinvocation_name,
           1577:  DEFVAR_LISP ("invocation-directory", &Vinvocation_directory,
           1581:  DEFVAR_LISP ("installation-directory", &Vinstallation_directory,
           1588:  DEFVAR_LISP ("system-messages-locale", &Vsystem_messages_locale,
           1592:  DEFVAR_LISP ("previous-system-messages-locale",
           1597:  DEFVAR_LISP ("system-time-locale", &Vsystem_time_locale,
           1601:  DEFVAR_LISP ("previous-system-time-locale", &Vprevious_system_time_locale,
           1605:  DEFVAR_LISP ("before-init-time", &Vbefore_init_time,
           1609:  DEFVAR_LISP ("after-init-time", &Vafter_init_time,
*/
        //  eval.c
                   quit_flag,
                   inhibit_quit,
                   stack_trace_on_error,
                   debug_on_error,
                   debug_ignored_errors,
                   debugger,
                   signal_hook_function,
                   debug_on_signal,
                   macro_declaration_function,
                   // fileio.c
                   file_name_coding_system,
                   default_file_name_coding_system,
/*                              5149:  DEFVAR_LISP ("directory-sep-char", &Vdirectory_sep_char,*/
                   file_name_handler_alist,
/*                              5168:  DEFVAR_LISP ("set-auto-coding-function",
                              5181:  DEFVAR_LISP ("after-insert-file-functions", &Vafter_insert_file_functions,
                              5191:  DEFVAR_LISP ("write-region-annotate-functions", &Vwrite_region_annotate_functions,
                              5219:  DEFVAR_LISP ("write-region-post-annotation-function",
                              5230:  DEFVAR_LISP ("write-region-annotations-so-far", */
                   inhibit_file_name_handlers,
                   inhibit_file_name_operation,
/*                              5246:  DEFVAR_LISP ("auto-save-list-file-name", &Vauto_save_list_file_name,
                              5253:  DEFVAR_LISP ("auto-save-visited-file-name", &Vauto_save_visited_file_name,
                           1 match for "defvar_lisp" in buffer: filelock.c
                               759:  DEFVAR_LISP ("temporary-file-directory", &Vtemporary_file_directory,
*/
                   // fns.c
                   features,
/*                           5 matches for "defvar_lisp" in buffer: font.c
                              5199:  DEFVAR_LISP ("font-encoding-alist", &Vfont_encoding_alist,
                              5221:  DEFVAR_LISP_NOPRO ("font-weight-table", &Vfont_weight_table,
                              5228:  DEFVAR_LISP_NOPRO ("font-slant-table", &Vfont_slant_table,
                              5233:  DEFVAR_LISP_NOPRO ("font-width-table", &Vfont_width_table,
                              5244:  DEFVAR_LISP ("font-log", &Vfont_log, doc: /*
                           7 matches for "defvar_lisp" in buffer: fontset.c
                              2224:  DEFVAR_LISP ("font-encoding-charset-alist", &Vfont_encoding_charset_alist,
                              2235:  DEFVAR_LISP ("use-default-ascent", &Vuse_default_ascent,
                              2245:  DEFVAR_LISP ("ignore-relative-composition", &Vignore_relative_composition,
                              2254:  DEFVAR_LISP ("alternate-fontname-alist", &Valternate_fontname_alist,
                              2260:  DEFVAR_LISP ("fontset-alias-alist", &Vfontset_alias_alist,
                              2266:  DEFVAR_LISP ("vertical-centering-font-regexp",
                              2273:  DEFVAR_LISP ("otf-script-alist", &Votf_script_alist,
                           9 matches for "defvar_lisp" in buffer: frame.c
                              4210:  DEFVAR_LISP ("x-resource-name", &Vx_resource_name,
                              4222:  DEFVAR_LISP ("x-resource-class", &Vx_resource_class,
                              4234:  DEFVAR_LISP ("frame-alpha-lower-limit", &Vframe_alpha_lower_limit,
                              4242:  DEFVAR_LISP ("default-frame-alist", &Vdefault_frame_alist,
                              4257:  DEFVAR_LISP ("default-frame-scroll-bars", &Vdefault_frame_scroll_bars,
                              4270:  DEFVAR_LISP ("terminal-frame", &Vterminal_frame,
                              4273:  DEFVAR_LISP ("mouse-position-function", &Vmouse_position_function,
                              4281:  DEFVAR_LISP ("mouse-highlight", &Vmouse_highlight,
                              4289:  DEFVAR_LISP ("delete-frame-functions", &Vdelete_frame_functions,
                           2 matches for "defvar_lisp" in buffer: fringe.c
                              1632:  DEFVAR_LISP ("overflow-newline-into-fringe", &Voverflow_newline_into_fringe,
                              1641:  DEFVAR_LISP ("fringe-bitmaps", &Vfringe_bitmaps,
                           5 matches for "defvar_lisp" in buffer: image.c
                              7880:  DEFVAR_LISP ("image-types", &Vimage_types,
                              7886:  DEFVAR_LISP ("image-library-alist", &Vimage_library_alist,
                              7900:  DEFVAR_LISP ("max-image-size", &Vmax_image_size,
                              8031:  DEFVAR_LISP ("x-bitmap-file-path", &Vx_bitmap_file_path,
                              8035:  DEFVAR_LISP ("image-cache-eviction-delay", &Vimage_cache_eviction_delay,
*/
                   // insdel.c
                   combine_after_change_calls,

//                           48 matches for "defvar_lisp" in buffer: keyboard.c
/*                             11241:  DEFVAR_LISP ("last-command-event", &last_command_event,
                             11244:  DEFVAR_LISP ("last-nonmenu-event", &last_nonmenu_event,
                             11250:  DEFVAR_LISP ("last-input-event", &last_input_event,
                             11253:  DEFVAR_LISP ("unread-command-events", &Vunread_command_events,
                             11264:  DEFVAR_LISP ("unread-post-input-method-events", &Vunread_post_input_method_events,
                             11270:  DEFVAR_LISP ("unread-input-method-events", &Vunread_input_method_events,
                             11278:  DEFVAR_LISP ("meta-prefix-char", &meta_prefix_char,
                             11308:  DEFVAR_LISP ("this-command", &Vthis_command,
                             11314:  DEFVAR_LISP ("this-command-keys-shift-translated",
                             11322:  DEFVAR_LISP ("this-original-command", &Vthis_original_command,
                             11334:  DEFVAR_LISP ("auto-save-timeout", &Vauto_save_timeout,
                             11341:  DEFVAR_LISP ("echo-keystrokes", &Vecho_keystrokes,
                             11353:  DEFVAR_LISP ("double-click-time", &Vdouble_click_time,
                             11386:  DEFVAR_LISP ("last-event-frame", &Vlast_event_frame,
                             11392:  DEFVAR_LISP ("tty-erase-char", &Vtty_erase_char,
                             11395:  DEFVAR_LISP ("help-char", &Vhelp_char,
                             11401:  DEFVAR_LISP ("help-event-list", &Vhelp_event_list,
                             11406:  DEFVAR_LISP ("help-form", &Vhelp_form,
                             11412:  DEFVAR_LISP ("prefix-help-command", &Vprefix_help_command, */
                             top_level,
/*                             11454:  DEFVAR_LISP ("menu-prompt-more-char", &menu_prompt_more_char, */
                             deactivate_mark,
/*                             11481:  DEFVAR_LISP ("command-hook-internal", &Vcommand_hook_internal,
                             11485:  DEFVAR_LISP ("pre-command-hook", &Vpre_command_hook,
                             11492:  DEFVAR_LISP ("post-command-hook", &Vpost_command_hook,
                             11500:  DEFVAR_LISP ("echo-area-clear-hook", ...,
                             11507:  DEFVAR_LISP ("lucid-menu-bar-dirty-flag", &Vlucid_menu_bar_dirty_flag,
                             11511:  DEFVAR_LISP ("menu-bar-final-items", &Vmenu_bar_final_items,
                             11530:  DEFVAR_LISP ("overriding-local-map", &Voverriding_local_map,
                             11536:  DEFVAR_LISP ("overriding-local-map-menu-flag", &Voverriding_local_map_menu_flag,
                             11542:  DEFVAR_LISP ("special-event-map", &Vspecial_event_map,
                             11546:  DEFVAR_LISP ("track-mouse", &do_mouse_tracking,
                             11606:  DEFVAR_LISP ("function-key-map", &Vfunction_key_map,
                             11614:  DEFVAR_LISP ("key-translation-map", &Vkey_translation_map,
                             11621:  DEFVAR_LISP ("deferred-action-list", &Vdeferred_action_list,
                             11626:  DEFVAR_LISP ("deferred-action-function", &Vdeferred_action_function,
                             11632:  DEFVAR_LISP ("suggest-key-bindings", &Vsuggest_key_bindings,
                             11638:  DEFVAR_LISP ("timer-list", &Vtimer_list,
                             11642:  DEFVAR_LISP ("timer-idle-list", &Vtimer_idle_list,
                             11646:  DEFVAR_LISP ("input-method-function", &Vinput_method_function,
                             11667:  DEFVAR_LISP ("input-method-previous-message",
                             11674:  DEFVAR_LISP ("show-help-function", &Vshow_help_function,
                             11679:  DEFVAR_LISP ("disable-point-adjustment", &Vdisable_point_adjustment,
                             11691:  DEFVAR_LISP ("global-disable-point-adjustment",
                             11700:  DEFVAR_LISP ("minibuffer-message-timeout", &Vminibuffer_message_timeout,
                             11705:  DEFVAR_LISP ("throw-on-input", &Vthrow_on_input,
                             11711:  DEFVAR_LISP ("command-error-function", &Vcommand_error_function,
                             11720:  DEFVAR_LISP ("enable-disabled-menus-and-buttons",
                           11 matches for "defvar_lisp" in buffer: keymap.c
                              3891:  DEFVAR_LISP ("define-key-rebound-commands", &Vdefine_key_rebound_commands,
                              3897:  DEFVAR_LISP ("minibuffer-local-map", &Vminibuffer_local_map,
                              3901:  DEFVAR_LISP ("minibuffer-local-ns-map", &Vminibuffer_local_ns_map,
                              3906:  DEFVAR_LISP ("minibuffer-local-completion-map", &Vminibuffer_local_completion_map,
                              3911:  DEFVAR_LISP ("minibuffer-local-filename-completion-map",
                              3919:  DEFVAR_LISP ("minibuffer-local-must-match-map", &Vminibuffer_local_must_match_map,
                              3925:  DEFVAR_LISP ("minibuffer-local-filename-must-match-map",
                              3932:  DEFVAR_LISP ("minor-mode-map-alist", &Vminor_mode_map_alist,
                              3940:  DEFVAR_LISP ("minor-mode-overriding-map-alist", &Vminor_mode_overriding_map_alist,
                              3947:  DEFVAR_LISP ("emulation-mode-map-alists", &Vemulation_mode_map_alists,
                              3956:  DEFVAR_LISP ("where-is-preferred-modifier", &Vwhere_is_preferred_modifier,
*/
                   // lread.c
                   obarray,
/*                              3968:  DEFVAR_LISP ("values", &Vvalues, */
                   standard_input,
                   read_with_symbol_positions,
                   read_symbol_positions_list,
/*                              4006:  DEFVAR_LISP ("load-path", &Vload_path,
                              4012:  DEFVAR_LISP ("load-suffixes", &Vload_suffixes,
                              4019:  DEFVAR_LISP ("load-file-rep-suffixes", &Vload_file_rep_suffixes,
                              4037:  DEFVAR_LISP ("after-load-alist", &Vafter_load_alist,
                              4052:  DEFVAR_LISP ("load-history", &Vload_history,*/
                   load_file_name,
/*                              4076:  DEFVAR_LISP ("user-init-file", &Vuser_init_file,
*/
                   current_load_list,
/*                              4089:  DEFVAR_LISP ("load-read-function", &Vload_read_function,
                              4094:  DEFVAR_LISP ("load-source-file-function", &Vload_source_file_function,
                              4114:  DEFVAR_LISP ("source-directory", &Vsource_directory,
                              4121:  DEFVAR_LISP ("preloaded-file-list", &Vpreloaded_file_list,
*/
                   byte_boolean_vars,
/*                              4136:  DEFVAR_LISP ("bytecomp-version-regexp", &Vbytecomp_version_regexp,
                              4145:  DEFVAR_LISP ("eval-buffer-list", &Veval_buffer_list,
*/
                   old_style_backquotes,

                   // macros.c
                   executing_kbd_macro,
/*                           15 matches for "defvar_lisp" in buffer: minibuf.c
                              2104:  DEFVAR_LISP ("read-buffer-function", &Vread_buffer_function,
                              2113:  DEFVAR_LISP ("minibuffer-setup-hook", &Vminibuffer_setup_hook,
                              2117:  DEFVAR_LISP ("minibuffer-exit-hook", &Vminibuffer_exit_hook,
                              2121:  DEFVAR_LISP ("history-length", &Vhistory_length,
                              2134:  DEFVAR_LISP ("history-add-new-input", &Vhistory_add_new_input,
                              2154:  DEFVAR_LISP ("minibuffer-completion-table", &Vminibuffer_completion_table,
                              2169:  DEFVAR_LISP ("minibuffer-completion-predicate", &Vminibuffer_completion_predicate,
                              2173:  DEFVAR_LISP ("minibuffer-completion-confirm", &Vminibuffer_completion_confirm,
                              2184:  DEFVAR_LISP ("minibuffer-completing-file-name",
                              2189:  DEFVAR_LISP ("minibuffer-help-form", &Vminibuffer_help_form,
                              2193:  DEFVAR_LISP ("minibuffer-history-variable", &Vminibuffer_history_variable,
                              2201:  DEFVAR_LISP ("minibuffer-history-position", &Vminibuffer_history_position,
                              2210:  DEFVAR_LISP ("completion-regexp-list", &Vcompletion_regexp_list,
                              2227:  DEFVAR_LISP ("minibuffer-prompt-properties", &Vminibuffer_prompt_properties,
                              2236:  DEFVAR_LISP ("read-expression-map", &Vread_expression_map,
                           1 match for "defvar_lisp" in buffer: msdos.c
                              5227:  DEFVAR_LISP ("dos-unsupported-char-glyph", &Vdos_unsupported_char_glyph,
                           */
        // 9 matches for "defvar_lisp" in buffer: print.c
        standard_output,
/*
   2296:  DEFVAR_LISP ("float-output-format", &Vfloat_output_format,
   2316:  DEFVAR_LISP ("print-length", &Vprint_length, */
        print_level,
        /*   2350:  DEFVAR_LISP ("print-gensym", &Vprint_gensym,
           2359:  DEFVAR_LISP ("print-circle", &Vprint_circle,
           2371:  DEFVAR_LISP ("print-continuous-numbering", &Vprint_continuous_numbering,
           2378:  DEFVAR_LISP ("print-number-table", &Vprint_number_table,
           2391:  DEFVAR_LISP ("print-charset-text-property", &Vprint_charset_text_property,
        2 matches for "defvar_lisp" in buffer: process.c
           6973:  DEFVAR_LISP ("process-connection-type", &Vprocess_connection_type,
           6982:  DEFVAR_LISP ("process-adaptive-read-buffering", &Vprocess_adaptive_read_buffering,
        2 matches for "defvar_lisp" in buffer: search.c */
        search_spaces_regexp,
        inhibit_changing_match_data, 
        // syntax.c
        find_word_boundary_function_table,
/*2 matches for "defvar_lisp" in buffer: term.c
    171://  DEFVAR_LISP ("suspend-tty-functions", &Vsuspend_tty_functions,
    178://  DEFVAR_LISP ("resume-tty-functions", &Vresume_tty_functions,
2 matches for "defvar_lisp" in buffer: terminal.c
    560:  DEFVAR_LISP ("ring-bell-function", &Vring_bell_function,
    565:  DEFVAR_LISP ("delete-terminal-functions", &Vdelete_terminal_functions, */
        // textprop.c
        default_text_properties,
        char_property_alias_alist,
        inhibit_point_motion_hooks,
        text_property_default_nonsticky,
/*2 matches for "defvar_lisp" in buffer: undo.c
    701:  DEFVAR_LISP ("undo-outer-limit", &Vundo_outer_limit,
    718:  DEFVAR_LISP ("undo-outer-limit-function", &Vundo_outer_limit_function,
2 matches for "defvar_lisp" in buffer: w16select.c
    732:  DEFVAR_LISP ("selection-coding-system", &Vselection_coding_system,
    739:  DEFVAR_LISP ("next-selection-coding-system", &Vnext_selection_coding_system,
22 matches for "defvar_lisp" in buffer: w32fns.c
   6800:  DEFVAR_LISP ("w32-color-map", &Vw32_color_map,
   6804:  DEFVAR_LISP ("w32-pass-alt-to-system", &Vw32_pass_alt_to_system,
   6811:  DEFVAR_LISP ("w32-alt-is-meta", &Vw32_alt_is_meta,
   6820:  DEFVAR_LISP ("w32-pass-lwindow-to-system",
   6835:  DEFVAR_LISP ("w32-pass-rwindow-to-system",
   6850:  DEFVAR_LISP ("w32-phantom-key-code",
   6862:  DEFVAR_LISP ("w32-enable-num-lock",
   6868:  DEFVAR_LISP ("w32-enable-caps-lock",
   6874:  DEFVAR_LISP ("w32-scroll-lock-modifier",
   6882:  DEFVAR_LISP ("w32-lwindow-modifier",
   6890:  DEFVAR_LISP ("w32-rwindow-modifier",
   6898:  DEFVAR_LISP ("w32-apps-modifier",
   6910:  DEFVAR_LISP ("w32-enable-palette", &Vw32_enable_palette,
   6965:  DEFVAR_LISP ("x-pointer-shape", &Vx_pointer_shape,
   6975:  DEFVAR_LISP ("x-hourglass-pointer-shape", &Vx_hourglass_pointer_shape,
   6981:  DEFVAR_LISP ("x-sensitive-text-pointer-shape",
   6988:  DEFVAR_LISP ("x-window-horizontal-drag-cursor",
   6996:  DEFVAR_LISP ("x-cursor-fore-pixel", &Vx_cursor_fore_pixel,
   7000:  DEFVAR_LISP ("x-max-tooltip-size", &Vx_max_tooltip_size,
   7005:  DEFVAR_LISP ("x-no-window-manager", &Vx_no_window_manager,
   7013:  DEFVAR_LISP ("x-pixel-size-width-font-regexp",
   7023:  DEFVAR_LISP ("w32-bdf-filename-alist",
1 match for "defvar_lisp" in buffer: w32font.c
   2602:  DEFVAR_LISP ("w32-charset-info-alist", */
        // w32proc.c
/*   2298:  DEFVAR_LISP ("w32-quote-process-args", &Vw32_quote_process_args,
   2310:  DEFVAR_LISP ("w32-start-process-show-window",
   2317:  DEFVAR_LISP ("w32-start-process-share-console",
   2327:  DEFVAR_LISP ("w32-start-process-inherit-error-mode", */
        w32_downcase_file_names,
/*   2354:  DEFVAR_LISP ("w32-generate-fake-inodes", &Vw32_generate_fake_inodes,
   2363:  DEFVAR_LISP ("w32-get-true-file-attributes", &Vw32_get_true_file_attributes,
2 matches for "defvar_lisp" in buffer: w32select.c
   1062:  DEFVAR_LISP ("selection-coding-system", &Vselection_coding_system,
   1072:  DEFVAR_LISP ("next-selection-coding-system", &Vnext_selection_coding_system,
5 matches for "defvar_lisp" in buffer: w32term.c
   6223:  DEFVAR_LISP ("w32-swap-mouse-buttons",
   6229:  DEFVAR_LISP ("w32-grab-focus-on-raise",
   6237:  DEFVAR_LISP ("w32-capslock-is-shiftlock",
   6243:  DEFVAR_LISP ("w32-recognize-altgr",
   6283:  DEFVAR_LISP ("x-toolkit-scroll-bars", &Vx_toolkit_scroll_bars, */
        // window.c
/*   7161:  DEFVAR_LISP ("temp-buffer-show-function", &Vtemp_buffer_show_function,
   7169:  DEFVAR_LISP ("minibuffer-scroll-window", &Vminibuf_scroll_window,
   7179:  DEFVAR_LISP ("other-window-scroll-buffer", &Vother_window_scroll_buffer,
   7209:  DEFVAR_LISP ("scroll-preserve-screen-position", */
        window_point_insertion_type,
/*   7224:  DEFVAR_LISP ("window-configuration-change-hook",
*/
// 32 matches for "defvar_lisp" in buffer: xdisp.c
        show_trailing_whitespace,
/*
  24255:  DEFVAR_LISP ("nobreak-char-display", &Vnobreak_char_display,
  24264:  DEFVAR_LISP ("void-text-area-pointer", &Vvoid_text_area_pointer,
  24270:  DEFVAR_LISP ("inhibit-redisplay", &Vinhibit_redisplay,
  24275:  DEFVAR_LISP ("global-mode-string", &Vglobal_mode_string,
  24279:  DEFVAR_LISP ("overlay-arrow-position", &Voverlay_arrow_position, */
        overlay_arrow_string,
        overlay_arrow_variable_list,
/*        
  24320:  DEFVAR_LISP ("display-pixels-per-inch",  &Vdisplay_pixels_per_inch,
  24325:  DEFVAR_LISP ("truncate-partial-width-windows",
  24346:  DEFVAR_LISP ("line-number-display-limit", &Vline_number_display_limit,
  24369:  DEFVAR_LISP ("frame-title-format", &Vframe_title_format,
  24377:  DEFVAR_LISP ("icon-title-format", &Vicon_title_format,
*/
        message_log_max,
/*  24400:  DEFVAR_LISP ("window-size-change-functions", &Vwindow_size_change_functions, */
        window_scroll_functions,
/*  24416:  DEFVAR_LISP ("window-text-change-functions",
  24421:  DEFVAR_LISP ("redisplay-end-trigger-functions", &Vredisplay_end_trigger_functions,
  24427:  DEFVAR_LISP ("mouse-autoselect-window", &Vmouse_autoselect_window,
  24447:  DEFVAR_LISP ("auto-resize-tool-bars", &Vauto_resize_tool_bars,
  24463:  DEFVAR_LISP ("tool-bar-border", &Vtool_bar_border,
  24471:  DEFVAR_LISP ("tool-bar-button-margin", &Vtool_bar_button_margin,
  24483:  DEFVAR_LISP ("fontification-functions", &Vfontification_functions,
  24500:  DEFVAR_LISP ("max-mini-window-height", &Vmax_mini_window_height,
  24506:  DEFVAR_LISP ("resize-mini-windows", &Vresize_mini_windows,
  24515:  DEFVAR_LISP ("blink-cursor-alist", &Vblink_cursor_alist,
  24538:  DEFVAR_LISP ("hscroll-step", &Vhscroll_step,
  24561:  DEFVAR_LISP ("menu-bar-update-hook",  &Vmenu_bar_update_hook,
  24568:  DEFVAR_LISP ("menu-updating-frame", &Vmenu_updating_frame,
  24577:  DEFVAR_LISP ("wrap-prefix", &Vwrap_prefix,
  24590:  DEFVAR_LISP ("line-prefix", &Vline_prefix,
  24630:  DEFVAR_LISP ("hourglass-delay", &Vhourglass_delay,
/*
8 matches for "defvar_lisp" in buffer: xfaces.c
   6053:  DEFVAR_LISP ("font-list-limit", &Vfont_list_limit,
   6059:  DEFVAR_LISP ("face-new-frame-defaults", &Vface_new_frame_defaults,
   6063:  DEFVAR_LISP ("face-default-stipple", &Vface_default_stipple,
   6070:  DEFVAR_LISP ("tty-defined-color-alist", &Vtty_defined_color_alist,
   6074:  DEFVAR_LISP ("scalable-fonts-allowed", &Vscalable_fonts_allowed,
   6084:  DEFVAR_LISP ("face-ignored-fonts", &Vface_ignored_fonts,
   6090:  DEFVAR_LISP ("face-remapping-alist", &Vface_remapping_alist,
   6127:  DEFVAR_LISP ("face-font-rescale-alist", &Vface_font_rescale_alist,
12 matches for "defvar_lisp" in buffer: xfns.c
   5818:  DEFVAR_LISP ("x-pointer-shape", &Vx_pointer_shape,
   5825:  DEFVAR_LISP ("x-nontext-pointer-shape", &Vx_nontext_pointer_shape,
   5832:  DEFVAR_LISP ("x-hourglass-pointer-shape", &Vx_hourglass_pointer_shape,
   5839:  DEFVAR_LISP ("x-mode-pointer-shape", &Vx_mode_pointer_shape,
   5846:  DEFVAR_LISP ("x-sensitive-text-pointer-shape",
   5853:  DEFVAR_LISP ("x-window-horizontal-drag-cursor",
   5860:  DEFVAR_LISP ("x-cursor-fore-pixel", &Vx_cursor_fore_pixel,
   5864:  DEFVAR_LISP ("x-max-tooltip-size", &Vx_max_tooltip_size,
   5869:  DEFVAR_LISP ("x-no-window-manager", &Vx_no_window_manager,
   5877:  DEFVAR_LISP ("x-pixel-size-width-font-regexp",
   5920:  DEFVAR_LISP ("motif-version-string", &Vmotif_version_string,
   5934:  DEFVAR_LISP ("gtk-version-string", &Vgtk_version_string,
3 matches for "defvar_lisp" in buffer: xselect.c
   2969:  DEFVAR_LISP ("selection-converter-alist", &Vselection_converter_alist,
   2984:  DEFVAR_LISP ("x-lost-selection-functions", &Vx_lost_selection_functions,
   2992:  DEFVAR_LISP ("x-sent-selection-functions", &Vx_sent_selection_functions,
2 matches for "defvar_lisp" in buffer: xsmfns.c
    542:  DEFVAR_LISP ("x-session-id", &Vx_session_id,
    550:  DEFVAR_LISP ("x-session-previous-id", &Vx_session_previous_id,
6 matches for "defvar_lisp" in buffer: xterm.c
  10900:  DEFVAR_LISP ("x-toolkit-scroll-bars", &Vx_toolkit_scroll_bars,
  10931:  DEFVAR_LISP ("x-alt-keysym", &Vx_alt_keysym,
  10938:  DEFVAR_LISP ("x-hyper-keysym", &Vx_hyper_keysym,
  10945:  DEFVAR_LISP ("x-meta-keysym", &Vx_meta_keysym,
  10952:  DEFVAR_LISP ("x-super-keysym", &Vx_super_keysym,
  10959:  DEFVAR_LISP ("x-keysym-table", &Vx_keysym_table,
4 matches for "defvar_lisp" in buffer: lisp.h
   1739:extern void defvar_lisp P_ ((char *, Lisp_Object *));
   1740:extern void defvar_lisp_nopro P_ ((char *, Lisp_Object *));
   1748:#define DEFVAR_LISP(lname, vname, doc) defvar_lisp (lname, vname)
   1749:#define DEFVAR_LISP_NOPRO(lname, vname, doc) defvar_lisp_nopro (lname, vname)
*/
        SIZE
    }

    public enum Ints
    {
        /*
        11 matches for "DEFVAR_INT" in buffer: alloc.c
           5471:  DEFVAR_INT ("gc-cons-threshold", &gc_cons_threshold,
           5489:  DEFVAR_INT ("pure-bytes-used", &pure_bytes_used,
           5492:  DEFVAR_INT ("cons-cells-consed", &cons_cells_consed,
           5495:  DEFVAR_INT ("floats-consed", &floats_consed,
           5498:  DEFVAR_INT ("vector-cells-consed", &vector_cells_consed,
           5501:  DEFVAR_INT ("symbols-consed", &symbols_consed,
           5504:  DEFVAR_INT ("string-chars-consed", &string_chars_consed,
           5507:  DEFVAR_INT ("misc-objects-consed", &misc_objects_consed,
           5510:  DEFVAR_INT ("intervals-consed", &intervals_consed,
           5513:  DEFVAR_INT ("strings-consed", &strings_consed,
           5551:  DEFVAR_INT ("gcs-done", &gcs_done,
        1 match for "DEFVAR_INT" in buffer: dispnew.c
           6251:  DEFVAR_INT ("baud-rate", &baud_rate,
        8 matches for "DEFVAR_INT" in buffer: dosfns.c
            707:  DEFVAR_INT ("dos-country-code", &dos_country_code,
            711:  DEFVAR_INT ("dos-codepage", &dos_codepage,
            723:  DEFVAR_INT ("dos-timezone-offset", &dos_timezone_offset,
            743:  DEFVAR_INT ("dos-hyper-key", &dos_hyper_key,
            748:  DEFVAR_INT ("dos-super-key", &dos_super_key,
            753:  DEFVAR_INT ("dos-keypad-mode", &dos_keypad_mode,
            777:  DEFVAR_INT ("dos-keyboard-layout", &dos_keyboard_layout,
            782:  DEFVAR_INT ("dos-decimal-point", &dos_decimal_point,
        1 match for "DEFVAR_INT" in buffer: emacs.c
           1556:  DEFVAR_INT ("emacs-priority", &emacs_priority,
*/
        // eval.c
        max_specpdl_size,
        max_lisp_eval_depth,
/*
        7 matches for "DEFVAR_INT" in buffer: keyboard.c
          11261:  DEFVAR_INT ("unread-command-char", &unread_command_char,
          11329:  DEFVAR_INT ("auto-save-interval", &auto_save_interval,
          11346:  DEFVAR_INT ("polling-period", &polling_period,
          11360:  DEFVAR_INT ("double-click-fuzz", &double_click_fuzz,
          11375:  DEFVAR_INT ("num-input-keys", &num_input_keys,
          11381:  DEFVAR_INT ("num-nonmacro-input-events", &num_nonmacro_input_events,
          11459:  DEFVAR_INT ("extra-keyboard-modifiers", &extra_keyboard_modifiers,
        1 match for "DEFVAR_INT" in buffer: lread.c
           3822:   DEFVAR_INT ("emacs-priority", &emacs_priority, "Documentation");  
        1 match for "DEFVAR_INT" in buffer: macros.c
            396:  DEFVAR_INT ("executing-kbd-macro-index", &executing_kbd_macro_index,
        2 matches for "DEFVAR_INT" in buffer: undo.c
            679:  DEFVAR_INT ("undo-limit", &undo_limit,
            689:  DEFVAR_INT ("undo-strong-limit", &undo_strong_limit,
        4 matches for "DEFVAR_INT" in buffer: w32fns.c
           6816:  DEFVAR_INT ("w32-quit-key", &w32_quit_key,
           6914:  DEFVAR_INT ("w32-mouse-button-tolerance",
           6923:  DEFVAR_INT ("w32-mouse-move-interval",
           7146:  DEFVAR_INT ("w32-ansi-code-page",
        1 match for "DEFVAR_INT" in buffer: w32proc.c
           2334:  DEFVAR_INT ("w32-pipe-read-delay", &w32_pipe_read_delay,
        1 match for "DEFVAR_INT" in buffer: w32term.c
           6218:  DEFVAR_INT ("w32-num-mouse-buttons",
        3 matches for "DEFVAR_INT" in buffer: window.c
           7187:  DEFVAR_INT ("next-screen-context-lines", &next_screen_context_lines,
           7191:  DEFVAR_INT ("window-min-height", &window_min_height,
           7200:  DEFVAR_INT ("window-min-width", &window_min_width,
        8 matches for "DEFVAR_INT" in buffer: xdisp.c
          24297:  DEFVAR_INT ("scroll-step", &scroll_step,
          24304:  DEFVAR_INT ("scroll-conservatively", &scroll_conservatively,
          24314:  DEFVAR_INT ("scroll-margin", &scroll_margin,
          24352:  DEFVAR_INT ("line-number-display-limit-width",
          24479:  DEFVAR_INT ("tool-bar-button-relief", &tool_bar_button_relief,
          24533:  DEFVAR_INT ("hscroll-margin", &hscroll_margin,
          24611:  DEFVAR_INT ("overline-margin", &overline_margin,
          24617:  DEFVAR_INT ("underline-minimum-offset",
        1 match for "DEFVAR_INT" in buffer: xselect.c
           3006:  DEFVAR_INT ("x-selection-timeout", &x_selection_timeout,
        1 match for "DEFVAR_INT" in buffer: lisp.h
           1751:#define DEFVAR_INT(lname, vname, doc) defvar_int (lname, vname)
        */
        SIZE
    }

    public enum Bools
    {
/*
1 match for "DEFVAR_BOOL" in buffer: alloc.c
   5520:  DEFVAR_BOOL ("garbage-collection-messages", &garbage_collection_messages,
1 match for "DEFVAR_BOOL" in buffer: bytecode.c
   1696:  DEFVAR_BOOL ("byte-metering-on", &byte_metering_on,
*/
        //charset.c
        inhibit_load_charset_map,
/*5 matches for "DEFVAR_BOOL" in buffer: coding.c
  10626:  DEFVAR_BOOL ("inhibit-eol-conversion", &inhibit_eol_conversion,
  10633:  DEFVAR_BOOL ("inherit-process-coding-system", &inherit_process_coding_system,
  10775:  DEFVAR_BOOL ("coding-system-require-warning",
  10784:  DEFVAR_BOOL ("inhibit-iso-escape-detection",
  10812:  DEFVAR_BOOL ("inhibit-null-byte-detection",
5 matches for "DEFVAR_BOOL" in buffer: dispnew.c
   6256:  DEFVAR_BOOL ("inverse-video", &inverse_video,
   6260:  DEFVAR_BOOL ("visible-bell", &visible_bell,
   6265:  DEFVAR_BOOL ("no-redraw-on-reenter", &no_redraw_on_reenter,
   6285:  DEFVAR_BOOL ("cursor-in-echo-area", &cursor_in_echo_area,
   6304:  DEFVAR_BOOL ("redisplay-dont-pause", &redisplay_dont_pause,
1 match for "DEFVAR_BOOL" in buffer: emacs.c
   1543:  DEFVAR_BOOL ("noninteractive", &noninteractive1,
*/
        // eval.c
        debug_on_quit,
        debug_on_next_call,
        debugger_may_continue,
/*
2 matches for "DEFVAR_BOOL" in buffer: fileio.c
   5259:  DEFVAR_BOOL ("write-region-inhibit-fsync", &write_region_inhibit_fsync,
   5266:  DEFVAR_BOOL ("delete-by-moving-to-trash", &delete_by_moving_to_trash,
2 matches for "DEFVAR_BOOL" in buffer: fns.c
   5165:  DEFVAR_BOOL ("use-dialog-box", &use_dialog_box,
   5174:  DEFVAR_BOOL ("use-file-dialog", &use_file_dialog,
1 match for "DEFVAR_BOOL" in buffer: frame.c
   4317:  DEFVAR_BOOL ("focus-follows-mouse", &focus_follows_mouse,
1 match for "DEFVAR_BOOL" in buffer: image.c
   8025:  DEFVAR_BOOL ("cross-disabled-images", &cross_disabled_images,*/
        // indent.c
        indent_tabs_mode,

        // insdel.c
        check_markers_debug_flag,
        inhibit_modification_hooks,

/*3 matches for "DEFVAR_BOOL" in buffer: keyboard.c
  11371:  DEFVAR_BOOL ("inhibit-local-menu-bar-menus", &inhibit_local_menu_bar_menus,
  11439:  DEFVAR_BOOL ("cannot-suspend", &cannot_suspend,
  11444:  DEFVAR_BOOL ("menu-prompting", &menu_prompting,
*/
        // lread.c
        load_in_progress,
        load_force_doc_strings,
        load_convert_to_unibyte,
        load_dangerous_libraries,
/*
1 match for "DEFVAR_BOOL" in buffer: marker.c
    923:  DEFVAR_BOOL ("byte-debug-flag", &byte_debug_flag,
6 matches for "DEFVAR_BOOL" in buffer: minibuf.c
   2108:  DEFVAR_BOOL ("read-buffer-completion-ignore-case",
   2128:  DEFVAR_BOOL ("history-delete-duplicates", &history_delete_duplicates,
   2141:  DEFVAR_BOOL ("completion-ignore-case", &completion_ignore_case,
   2149:  DEFVAR_BOOL ("enable-recursive-minibuffers", &enable_recursive_minibuffers,
   2205:  DEFVAR_BOOL ("minibuffer-auto-raise", &minibuffer_auto_raise,
   2219:  DEFVAR_BOOL ("minibuffer-allow-text-properties",
1 match for "DEFVAR_BOOL" in buffer: msdos.c
   5234:  DEFVAR_BOOL ("delete-exited-processes", &delete_exited_processes,
4 matches for "DEFVAR_BOOL" in buffer: print.c
   2326:  DEFVAR_BOOL ("print-escape-newlines", &print_escape_newlines,
   2331:  DEFVAR_BOOL ("print-escape-nonascii", &print_escape_nonascii,
   2339:  DEFVAR_BOOL ("print-escape-multibyte", &print_escape_multibyte,
   2345:  DEFVAR_BOOL ("print-quoted", &print_quoted,
1 match for "DEFVAR_BOOL" in buffer: process.c
   6967:  DEFVAR_BOOL ("delete-exited-processes", &delete_exited_processes, */
        // syntax.c
/*   3440:  DEFVAR_BOOL ("parse-sexp-ignore-comments", &parse_sexp_ignore_comments, */
        parse_sexp_lookup_properties, 
        words_include_escapes,
/*   3453:  DEFVAR_BOOL ("multibyte-syntax-as-symbol", &multibyte_syntax_as_symbol,
   3457:  DEFVAR_BOOL ("open-paren-in-column-0-is-defun-start", 
2 matches for "DEFVAR_BOOL" in buffer: term.c
    162://  DEFVAR_BOOL ("system-uses-terminfo", &system_uses_terminfo,
    184://  DEFVAR_BOOL ("visible-cursor", &visible_cursor,*/
        // undo.c
        undo_inhibit_record_point,
/*1 match for "DEFVAR_BOOL" in buffer: w32console.c
    170:  DEFVAR_BOOL ("w32-use-full-screen-buffer",
5 matches for "DEFVAR_BOOL" in buffer: w32fns.c
   6906:  DEFVAR_BOOL ("w32-enable-synthesized-fonts", &w32_enable_synthesized_fonts,
   6931:  DEFVAR_BOOL ("w32-pass-extra-mouse-buttons-to-system",
   6941:  DEFVAR_BOOL ("w32-pass-multimedia-buttons-to-system",
   7028:  DEFVAR_BOOL ("w32-strict-fontnames",
   7038:  DEFVAR_BOOL ("w32-strict-painting",
3 matches for "DEFVAR_BOOL" in buffer: w32term.c
   6250:  DEFVAR_BOOL ("w32-use-visible-system-caret",
   6267:  DEFVAR_BOOL ("x-use-underline-position-properties",
   6275:  DEFVAR_BOOL ("x-underline-at-descent-line",
*/
        // window.c
        mode_line_in_non_selected_windows,
        auto_window_vscroll_p,
/*14 matches for "DEFVAR_BOOL" in buffer: xdisp.c
    614:/* The symbol `inhibit-menubar-update' and its DEFVAR_BOOL variable.  
  24243:  DEFVAR_BOOL ("x-stretch-cursor", &x_stretch_cursor_p,
  24340:  DEFVAR_BOOL ("mode-line-inverse-video", &mode_line_inverse_video,
  24359:  DEFVAR_BOOL ("highlight-nonselected-windows", &highlight_nonselected_windows,
  24363:  DEFVAR_BOOL ("multiple-frames", &multiple_frames,
  24455:  DEFVAR_BOOL ("auto-raise-tool-bar-buttons", &auto_raise_tool_bar_buttons_p,
  24459:  DEFVAR_BOOL ("make-cursor-line-fully-visible", &make_cursor_line_fully_visible_p,
  24491:  DEFVAR_BOOL ("unibyte-display-via-language-environment",
  24527:  DEFVAR_BOOL ("auto-hscroll-mode", &automatic_hscrolling_p,
  24556:  DEFVAR_BOOL ("message-truncate-lines", &message_truncate_lines,
  24573:  DEFVAR_BOOL ("inhibit-menubar-update", &inhibit_menubar_update, */
        inhibit_eval_during_redisplay,
/*  24607:  DEFVAR_BOOL ("inhibit-free-realized-faces", &inhibit_free_realized_faces,
  24626:  DEFVAR_BOOL ("display-hourglass", &display_hourglass_p,
4 matches for "DEFVAR_BOOL" in buffer: xfns.c
   5888:  DEFVAR_BOOL ("x-gtk-use-old-file-dialog", &x_gtk_use_old_file_dialog,
   5895:  DEFVAR_BOOL ("x-gtk-show-hidden-files", &x_gtk_show_hidden_files,
   5901:  DEFVAR_BOOL ("x-gtk-file-dialog-help-text", &x_gtk_file_dialog_help_text,
   5907:  DEFVAR_BOOL ("x-gtk-whole-detached-tool-bar", &x_gtk_whole_detached_tool_bar,
3 matches for "DEFVAR_BOOL" in buffer: xterm.c
  10872:  DEFVAR_BOOL ("x-use-underline-position-properties",
  10882:  DEFVAR_BOOL ("x-underline-at-descent-line",
  10890:  DEFVAR_BOOL ("x-mouse-click-focus-ignore-position",
1 match for "DEFVAR_BOOL" in buffer: lisp.h
   1750:#define DEFVAR_BOOL(lname, vname, doc) defvar_bool (lname, vname)*/
        SIZE
    }

    public class Defs : Indexable<int>, Indexable<LispObject>, Indexable<bool>
    {
        public static LispObject[] O = new LispObject[(int)Objects.SIZE];
        public static int[] I = new int[(int)Ints.SIZE];
        public static bool[] B = new bool[(int)Bools.SIZE];

        int Indexable<int>.this[int index]
        {
            get { return I[index]; }
            set { I[index] = value; }
        }

        LispObject Indexable<LispObject>.this[int index]
        {
            get { return O[index]; }
            set { O[index] = value; }
        }

        bool Indexable<bool>.this[int index]
        {
            get { return B[index]; }
            set { B[index] = value; }
        }

        public static Defs Instance = new Defs();
    }    
}