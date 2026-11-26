// Even Stronger - the Cyber Field with Kris + Noelle.
//   flags: 2460-2464 = puzzle frozen by Noelle (keyboard 1, keyboard 2, keyboard 3, musical door, viro ring)

function scr_es_incyber()
{
    return string_pos("room_dw_cyber", room_get_name(room)) == 1;
}

// the field gets quieter the more you kill, and silent once it's empty (or Sweet Cap'n Cakes is done)
function scr_es_cyber_music_gone()
{
    return global.plot >= 60 || global.flag[2452] >= 12;
}

function scr_es_cyber_music_tick()
{
    if (!variable_global_exists("es_cybersong") || global.fighting == 1)
        exit;
    if (!audio_is_playing(global.es_cybersong))
        exit;
    if (!scr_es_incyber() || scr_es_cyber_music_gone())
    {
        if (scr_es_cyber_music_gone())
            audio_stop_sound(global.es_cybersong);
        exit;
    }
    var k = global.flag[2452];
    audio_sound_pitch(global.es_cybersong, max(0.55, 0.82 - (k * 0.025)));
    audio_sound_gain(global.es_cybersong, clamp(1 - (k / 12), 0.15, 1) * global.flag[16], 0);
}

// puzzle table: room -> [the game's own "solved" flag, Noelle's flag]
function scr_es_puzzle_flags()
{
    switch (room)
    {
        case room_dw_cyber_keyboard_puzzle_1: return [390, 2460];
        case room_dw_cyber_keyboard_puzzle_2: return [333, 2461];
        case room_dw_cyber_keyboard_puzzle_3: return [420, 2462];
        case room_dw_cyber_musical_door: return [332, 2463];
        case room_dw_cyber_viro_ring: return [352, 2464];
    }
    return -1;
}

// runs a couple of frames after every room starts
function scr_es_roomstart()
{
    var n = room_get_name(room);
    // the locals hid or fled: no NPCs in the Cyber Field or the city
    if (string_pos("room_dw_cyber", n) == 1 || string_pos("room_dw_city", n) == 1)
    {
        with (obj_npc_room)
            instance_destroy();
        with (obj_npc_room_animated)
            instance_destroy();
        with (obj_npc_facing)
            instance_destroy();
    }
    // puzzles: Noelle freezes them (or already has)
    var pf = scr_es_puzzle_flags();
    if (is_array(pf))
    {
        if (global.flag[pf[0]] == 0)
        {
            if (!instance_exists(obj_es_freeze))
                instance_create(0, 0, obj_es_freeze);
        }
        else if (global.flag[pf[1]] == 1)
        {
            if (!instance_exists(obj_es_frost))
                instance_create(0, 0, obj_es_frost);
        }
    }
}

// the things a puzzle is made of (for aiming the ice and drawing the frost)
function scr_es_puzzle_parts()
{
    var parts = [];
    with (obj_ch2_keyboardpuzzle_tile)
        array_push(parts, id);
    with (obj_npc_musical_door)
        array_push(parts, id);
    with (obj_ow_viroring)
        array_push(parts, id);
    with (obj_forcefield)
        array_push(parts, id);
    if (array_length(parts) == 0)
    {
        with (obj_ch2_keyboardpuzzle_controller)
            array_push(parts, id);
    }
    return parts;
}
