// dev-only test harness (only compiled with --test). Reads es_test.txt from the save folder and
// plays it back: key presses, waits, warps, screenshots. Lets the mod be playtested headless.
function scr_es_test_tick()
{
    if (!variable_global_exists("es_t_lines"))
    {
        global.es_t_lines = [];
        global.es_t_i = 0;
        global.es_t_wait = 0;
        global.es_t_frame = 0;
        global.es_t_release = [];
        global.es_t_log = "";
        global.es_t_mash = 0;
        global.es_t_until = "";
        global.es_t_mashleft = 0;
        if (file_exists("es_test.txt"))
        {
            var f = file_text_open_read("es_test.txt");
            while (!file_text_eof(f))
            {
                array_push(global.es_t_lines, string(file_text_read_string(f)));
                file_text_readln(f);
            }
            file_text_close(f);
        }
    }
    global.es_t_frame++;
    for (var r = array_length(global.es_t_release) - 1; r >= 0; r--)
    {
        var rel = global.es_t_release[r];
        rel[1] -= 1;
        global.es_t_release[r] = rel;
        if (rel[1] <= 0)
        {
            keyboard_key_release(rel[0]);
            array_delete(global.es_t_release, r, 1);
        }
    }
    if (variable_global_exists("es_t_mashleft") && global.es_t_mashleft > 0)
    {
        global.es_t_mashleft--;
        if ((global.es_t_mashleft mod 12) == 0)
        {
            keyboard_key_press(ord("Z"));
            array_push(global.es_t_release, [ord("Z"), 2]);
        }
        exit;
    }
    if (global.es_t_wait > 0)
    {
        global.es_t_wait--;
        exit;
    }
    if (variable_global_exists("es_t_until") && global.es_t_until != "")
    {
        var notyet;
        if (global.es_t_until == "!fight")
            notyet = !(instance_exists(obj_battlecontroller) && global.fighting == 1 && global.bmenuno == 0 && global.mnfight == 0);
        else if (string_char_at(global.es_t_until, 1) == "#")
            notyet = global.plot < real(string_delete(global.es_t_until, 1, 1));
        else
            notyet = room_get_name(room) != global.es_t_until;
        if (notyet)
        {
            if (global.es_t_mash && (global.es_t_frame mod 12) == 0)
            {
                keyboard_key_press(ord("Z"));
                array_push(global.es_t_release, [ord("Z"), 2]);
            }
            exit;
        }
        global.es_t_until = "";
        global.es_t_wait = 20;
        exit;
    }
    var guard = 0;
    while (global.es_t_i < array_length(global.es_t_lines) && global.es_t_wait <= 0 && guard < 50)
    {
        guard++;
        var line = string_trim(global.es_t_lines[global.es_t_i]);
        global.es_t_i++;
        if (line == "" || string_char_at(line, 1) == "#")
            continue;
        var a = string_split(line, " ", true);
        var c = a[0];
        var n1 = (array_length(a) > 1) ? a[1] : "";
        var n2 = (array_length(a) > 2) ? a[2] : "";
        switch (c)
        {
            case "w":
                global.es_t_wait = real(n1);
                break;
            case "k":
                var vk = scr_es_test_key(n1);
                keyboard_key_press(vk);
                array_push(global.es_t_release, [vk, (n2 == "") ? 2 : real(n2)]);
                global.es_t_wait = (n2 == "") ? 4 : real(n2) + 2;
                break;
            case "untilfight":
                global.es_t_mash = 1;
                global.es_t_until = "!fight";
                global.es_t_wait = 1;
                break;
            case "untilplot":
                global.es_t_mash = 1;
                global.es_t_until = "#" + n1;
                global.es_t_wait = 1;
                break;
            case "until":
            case "mash":
                global.es_t_mash = (c == "mash");
                global.es_t_until = n1;
                global.es_t_wait = 1;
                break;
            case "mashfor":
                // press z every 12 frames for n1 frames
                global.es_t_mashleft = real(n1);
                break;
            case "fight":
                // start encounter n1 right here (overworld)
                scr_battle(real(n1), 0, noone, noone, noone);
                global.es_t_wait = 30;
                break;
            case "room":
                room_goto(asset_get_index(n1));
                global.es_t_wait = 10;
                break;
            case "plot":
                global.plot = real(n1);
                break;
            case "flag":
                global.flag[real(n1)] = real(n2);
                break;
            case "char":
                global.char[real(n1)] = real(n2);
                break;
            case "killall":
                // freeze every enemy through the game's own defeat path
                if (instance_exists(obj_battlecontroller))
                for (var ki = 0; ki < 3; ki++)
                {
                    if (global.monster[ki] == 1 && i_ex(global.monsterinstance[ki]))
                    {
                        global.monsterhp[ki] = 0;
                        global.flag[51 + ki] = 6;
                        with (global.monsterinstance[ki])
                            scr_monsterdefeat();
                    }
                }
                break;
            case "sweetturns":
                global.es_sweet_turns = real(n1);
                break;
            case "hp":
                global.hp[1] = real(n1);
                global.hp[4] = real(n1);
                break;
            case "monhp":
                for (var mi = 0; mi < 3; mi++)
                    if (global.monster[mi] == 1)
                        global.monsterhp[mi] = real(n1);
                break;
            case "tp":
                global.tension = real(n1);
                break;
            case "pos":
                if (instance_exists(obj_mainchara))
                {
                    obj_mainchara.x = real(n1);
                    obj_mainchara.y = real(n2);
                }
                break;
            case "shot":
                screen_save("es_" + n1 + ".png");
                scr_es_test_log("shot " + n1);
                break;
            case "log":
                scr_es_test_log(string_copy(line, 5, 999));
                break;
            case "state":
                scr_es_test_log("room=" + room_get_name(room) + " plot=" + string(global.plot) + " fighting=" + string(global.fighting) + " hp=" + string(global.hp[1]) + "," + string(global.hp[4]) + " tp=" + string(global.tension) + " char=" + string(global.char[0]) + "," + string(global.char[1]) + "," + string(global.char[2]) + " mon=" + string(global.monster[0]) + ":" + string(global.monsterhp[0]) + "/" + string(global.monstermaxhp[0]) + "," + string(global.monster[1]) + ":" + string(global.monsterhp[1]) + "/" + string(global.monstermaxhp[1]) + " kills=" + string(global.flag[2452]) + " lv=" + string(global.flag[65]) + " spd=" + string(room_speed) + " mn=" + string(global.mnfight) + " kris=" + (instance_exists(obj_mainchara) ? (string(obj_mainchara.x) + "," + string(obj_mainchara.y)) : "-") + " face=" + string(global.facing) + " int=" + string(global.interact) + " cs=" + string(instance_exists(obj_cutscene_master)));
                break;
            case "mus":
                var ms = "streams:";
                with (obj_astream)
                    ms += " " + string(songname) + "(" + string(mystream) + ")";
                scr_es_test_log(ms + " wd=" + working_directory);
                break;
            case "quit":
                scr_es_test_log("quit");
                game_end();
                break;
            default:
                scr_es_test_log("?? " + line);
        }
    }
}

function scr_es_test_key(argument0)
{
    switch (argument0)
    {
        case "z": return ord("Z");
        case "x": return ord("X");
        case "c": return ord("C");
        case "up": return vk_up;
        case "down": return vk_down;
        case "left": return vk_left;
        case "right": return vk_right;
        case "enter": return vk_enter;
        case "shift": return vk_shift;
    }
    return ord(string_upper(argument0));
}

function scr_es_test_log(argument0)
{
    var f = file_text_open_append("es_log.txt");
    file_text_write_string(f, string(global.es_t_frame) + ": " + string(argument0));
    file_text_writeln(f);
    file_text_close(f);
}
