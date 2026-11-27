// Sprites and sounds borrowed from the player's own Chapter 3 (see assets.txt). The patcher
// exports them to even_stronger/; they're loaded once, on first use.

function scr_es_spr(_name)
{
    if (!variable_global_exists("es_sprmap"))
        global.es_sprmap = ds_map_create();
    if (ds_map_exists(global.es_sprmap, _name))
        return ds_map_find_value(global.es_sprmap, _name);
    var dir = working_directory + "even_stronger/";
    var spr = spr_pixel_white;
    if (file_exists(dir + _name + ".png") && file_exists(dir + _name + ".txt"))
    {
        var f = file_text_open_read(dir + _name + ".txt");
        var info = string_split(file_text_read_string(f), " ", true);
        file_text_close(f);
        var n = real(info[0]);
        var ox = real(info[1]);
        var oy = real(info[2]);
        var s = sprite_add(dir + _name + ".png", n, false, false, ox, oy);
        if (s >= 0)
            spr = s;
    }
    ds_map_set(global.es_sprmap, _name, spr);
    return spr;
}

// plays a borrowed sound (ogg or wav); returns the playing instance, or -1
function scr_es_snd(_name, _vol = 1, _pitch = 1)
{
    if (!variable_global_exists("es_sndmap"))
        global.es_sndmap = ds_map_create();
    var snd;
    if (ds_map_exists(global.es_sndmap, _name))
    {
        snd = ds_map_find_value(global.es_sndmap, _name);
    }
    else
    {
        snd = -1;
        var dir = working_directory + "even_stronger/";
        if (file_exists(dir + _name + ".ogg"))
            snd = audio_create_stream(dir + _name + ".ogg");
        else if (file_exists(dir + _name + ".wav"))
            snd = scr_es_load_wav(dir + _name + ".wav");
        ds_map_set(global.es_sndmap, _name, snd);
    }
    if (snd == -1)
        return -1;
    var inst = audio_play_sound(snd, 60, false);
    audio_sound_gain(inst, _vol, 0);
    audio_sound_pitch(inst, _pitch);
    return inst;
}

function scr_es_load_wav(_path)
{
    var b = buffer_load(_path);
    if (b < 0)
        return -1;
    var size = buffer_get_size(b);
    var pos = 12;
    var ch = 1;
    var rate = 44100;
    var bits = 16;
    var dpos = -1;
    var dlen = 0;
    while (pos + 8 <= size)
    {
        var id0 = chr(buffer_peek(b, pos, buffer_u8)) + chr(buffer_peek(b, pos + 1, buffer_u8)) + chr(buffer_peek(b, pos + 2, buffer_u8)) + chr(buffer_peek(b, pos + 3, buffer_u8));
        var clen = buffer_peek(b, pos + 4, buffer_u32);
        if (id0 == "fmt ")
        {
            ch = buffer_peek(b, pos + 10, buffer_u16);
            rate = buffer_peek(b, pos + 12, buffer_u32);
            bits = buffer_peek(b, pos + 22, buffer_u16);
        }
        if (id0 == "data")
        {
            dpos = pos + 8;
            dlen = min(clen, size - dpos);
            break;
        }
        pos += 8 + clen + (clen mod 2);
    }
    if (dpos < 0)
        return -1;
    return audio_create_buffer_sound(b, (bits == 8) ? buffer_u8 : buffer_s16, rate, dpos, dlen, (ch == 2) ? audio_stereo : audio_mono);
}
