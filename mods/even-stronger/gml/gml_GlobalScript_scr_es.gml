// DT Chapter 2: Even Stronger - core rules.
//
// save flags used by the mod (the game saves flags 0-2499, vanilla Ch2 never goes past ~960):
//   2450  first-ability easter egg seen (Pain)
//   2451  Queen rules active (FIGHT works, abilities hit harder)
//   2452  kills this playthrough
//   2453  opening done
//   2454  freezes this playthrough
//   2455  Noelle has killed (outside the opening fight): weird-route sprites from now on
//
// spells added: 40 X-Slash (Kris), 41 Slashrush (Kris), 42 FrostNova (Noelle)

function scr_es_on()
{
    return 1;
}

// once a new file starts: Kris alone, Noelle joins in the library
function scr_es_newgame()
{
    global.char[0] = 1;
    global.char[1] = 0;
    global.char[2] = 0;
    scr_es_setspells();
}

function scr_es_setspells()
{
    for (var j = 0; j < 12; j++)
    {
        global.spell[1][j] = 0;
        global.spell[4][j] = 0;
    }
    global.spell[1][0] = 40;
    global.spell[1][1] = 41;
    global.spell[4][0] = 9;
    global.spell[4][1] = 42;
    global.spell[4][2] = 2;
}

// FIGHT: Kris always misses, Noelle barely scratches (and can never land the kill)
function scr_es_fightdmg(argument0)
{
    var d = argument0;
    // BlackKnife: Kris's FIGHT works again, and SlayCyber hits Darkners 1.5x
    if (char == 1 && scr_es_blackknife() && scr_es_darkner(global.chartarget[myself]))
        d = ceil(d * 1.5);
    // the Roaring Knight: FIGHT works, but the party hits 20% softer and FIGHT another 3% softer
    if (scr_es_knightfight())
        return max(1, round(d * 0.8 * 0.97));
    if (global.flag[2451] == 1)
        return d;
    if (char == 1 && scr_es_blackknife())
        return d;
    if (char == 1)
    {
        scr_es_try_snap();
        return 0;
    }
    if (char == 4)
    {
        d = ceil(d * 0.12);
        var t = global.chartarget[myself];
        d = min(d, global.monsterhp[t] - 1);
        return max(d, 0);
    }
    return d;
}

// ability damage multiplier (Queen: extra damage)
function scr_es_abilitymul()
{
    if (scr_es_knightfight())
        return 0.8;
    return (global.flag[2451] == 1) ? 1.35 : 1;
}

function scr_es_knightfight()
{
    return variable_global_exists("es_kfight") && global.es_kfight == 1;
}

// a slash is about to hit monster slot argument0 for argument1 damage: make it a real kill
function scr_es_markslashkill(argument0, argument1)
{
    if (argument1 >= global.monsterhp[argument0] && i_ex(global.monsterinstance[argument0]))
    {
        if (global.flag[51 + argument0] == 0)
            global.flag[51 + argument0] = 1;
        with (global.monsterinstance[argument0])
            fatal = 1;
        global.flag[63] = 1;
    }
}

// counts kills for the story (called from scr_monsterdefeat)
function scr_es_countkill(argument0)
{
    if (global.flag[51 + argument0] == 1 || global.flag[51 + argument0] == 6)
        global.flag[2452] += 1;
    if (global.flag[51 + argument0] == 6)
    {
        global.flag[2454] += 1;
        // Noelle's first kill after the opening fight: she starts to change
        if (global.plot >= 51)
            global.flag[2455] = 1;
    }
}

function scr_es_spellinfo(argument0)
{
    switch (argument0)
    {
        case 40:
            spellname = "X-Slash";
            spellnameb = "X-Slash";
            spelldescb = "Physical#damage";
            spelldesc = "Two crossing slashes.#Depends on Attack.";
            spelltarget = 2;
            cost = 62;
            spellusable = 0;
            return 1;
        case 41:
            spellname = "Slashrush";
            spellnameb = "Slashrush";
            spelldescb = "Mash Z!#80% TP";
            spelldesc = "Slash as fast as you can.#Press Z to hit.";
            spelltarget = 2;
            cost = 200;
            spellusable = 0;
            return 1;
        case 42:
            spellname = "FrostNova";
            spellnameb = "FrostNova";
            spelldescb = "ICE dmg#all foes";
            spelldesc = "A giant snowflake rains on#every enemy. Not SnowGrave.";
            spelltarget = 0;
            cost = 150;
            spellusable = 0;
            return 1;
    }
    return 0;
}

function scr_es_spelltext(argument0, argument1)
{
    var nm = global.charname[global.char[argument1]];
    switch (argument0)
    {
        case 40:
            global.msg[0] = "* " + nm + " used X-SLASH!/%";
            return 1;
        case 41:
            global.msg[0] = "* " + nm + " used SLASHRUSH!&* (Mash Z!)/%";
            return 1;
        case 42:
            global.msg[0] = "* " + nm + " cast FROSTNOVA!/%";
            return 1;
    }
    return 0;
}

// runs inside scr_spell, in the casting hero. returns 1 when it handled the spell.
function scr_es_spell(argument0, argument1)
{
    var sp = argument0;
    if (sp != 9 && sp != 40 && sp != 41 && sp != 42)
        return 0;
    // the first ability ever used: Pain.
    if (global.flag[2450] == 0 && !(variable_global_exists("es_pain_bypass") && global.es_pain_bypass == 1))
    {
        global.flag[2450] = 1;
        var p = instance_create(0, 0, obj_es_pain);
        p.spell = sp;
        p.caster = argument1;
        p.hero = id;
        global.spelldelay = 99999;
        return 1;
    }
    if (sp == 9)
        return 0;
    if ((sp == 40 || sp == 41) && scr_es_try_snap())
        return 1;
    var st = global.chartarget[argument1];
    cancelattack = 0;
    if (sp != 42 && global.monster[st] == 0)
        scr_retarget_spell();
    if (cancelattack == 1)
        return 1;
    st = global.chartarget[argument1];
    var e;
    switch (sp)
    {
        case 40:
            e = instance_create(0, 0, obj_es_xslash);
            e.star = st;
            e.caster = argument1;
            global.spelldelay = 50;
            break;
        case 41:
            e = instance_create(0, 0, obj_es_slashrush);
            e.star = st;
            e.caster = argument1;
            global.spelldelay = 99999;
            break;
        case 42:
            e = instance_create(0, 0, obj_es_frostnova);
            e.caster = argument1;
            e.damage = ceil(((global.battlemag[argument1] * 12) + 40 + random(10)) * scr_es_abilitymul());
            global.spelldelay = 150;
            break;
    }
    return 1;
}

// X-Slash damage, straight from Spamton NEO's X-Slash act
function scr_es_xslashdmg(argument0, argument1)
{
    var d = max(1, round(((((global.battleat[argument0] * 150) / 20) - (global.monsterdf[argument1] * 3)) * 1.25) * scr_es_abilitymul()));
    if (global.char[argument0] == 1 && scr_es_blackknife() && scr_es_darkner(argument1))
        d = ceil(d * 1.5);
    return d;
}

// Kris is holding the knight's sword
function scr_es_blackknife()
{
    return global.charweapon[1] == 23;
}

// SlayCyber: everyone in this world is a Darkner, except the Lightners (Berdly)
function scr_es_darkner(argument0)
{
    if (argument0 < 0 || argument0 > 2 || !i_ex(global.monsterinstance[argument0]))
        return true;
    var o = global.monsterinstance[argument0].object_index;
    return !(o == obj_berdlyb_enemy || o == obj_berdlyb2_enemy || o == obj_berdlyplug_enemy);
}

function scr_es_cast_after_pain(argument0, argument1, argument2)
{
    global.es_pain_bypass = 1;
    with (argument2)
        scr_spell(argument0, argument1);
    global.es_pain_bypass = 0;
}

// No mercy in this route: every enemy's ACT list is just Check plus Kris's skills
// (X-Slash, Slashrush). The enemy's own acts and everyone's S/N/R-Actions are removed.
// Spamton NEO keeps his own list (his X-Slash is already in it).
function scr_es_addkrisacts()
{
    if (!scr_havechar(1))
        exit;
    for (var i = 0; i < 3; i++)
    {
        if (global.monster[i] != 1 || global.monstertype[i] == 50)
            continue;
        // already in shape?
        if (global.canact[i][1] == 1 && global.actname[i][1] == "X-Slash" && global.canact[i][2] == 1 && global.actname[i][2] == "Slashrush" && global.canact[i][3] == 0)
            continue;
        var hascheck = (global.canact[i][0] == 1 && global.actname[i][0] == "Check");
        for (var k = 0; k < 6; k++)
        {
            if (!(k == 0 && hascheck))
                global.canact[i][k] = 0;
            global.canactsus[i][k] = 0;
            global.canactral[i][k] = 0;
            global.canactnoe[i][k] = 0;
        }
        var at = hascheck ? 1 : 0;
        global.canact[i][at] = 1;
        global.actname[i][at] = "X-Slash";
        global.actdesc[i][at] = "Physical#damage";
        global.actcost[i][at] = 62;
        global.actactor[i][at] = 1;
        global.actsimul[i][at] = 0;
        global.canact[i][at + 1] = 1;
        global.actname[i][at + 1] = "Slashrush";
        global.actdesc[i][at + 1] = "Mash Z!";
        global.actcost[i][at + 1] = 200;
        global.actactor[i][at + 1] = 1;
        global.actsimul[i][at + 1] = 0;
    }
}

// called at the top of scr_actselect: Kris picking one of his skills casts it as a spell instead
function scr_es_actselect(argument0, argument1)
{
    if (global.char[global.charturn] != 1)
        return 0;
    var an = global.actname[argument0][argument1];
    if (an != "X-Slash" && an != "Slashrush")
        return 0;
    if (i_ex(global.monsterinstance[argument0]) && global.monstertype[argument0] == 50)
        return 0;
    global.chartarget[global.charturn] = argument0;
    global.faceaction[global.charturn] = 2;
    global.charaction[global.charturn] = 2;
    global.charspecial[global.charturn] = (an == "X-Slash") ? 40 : 41;
    return 1;
}

// every battle's box text is one of three lines, picked fresh each turn
function scr_es_flavor()
{
    if (!variable_global_exists("es_flv_src"))
    {
        global.es_flv_src = "";
        global.es_flv = "";
        global.es_flv_last = -1;
    }
    // a new turn (the game set new flavor text) or a new battle: pick again
    if (global.battlemsg[0] != global.es_flv_src || global.es_flv == "")
    {
        global.es_flv_src = global.battlemsg[0];
        var lines = ["* Hold.", "* Its burning..", "* You are filled with power."];
        // the Roaring Knight has its own lines, and says it when someone falls
        if (scr_es_knightfight())
        {
            lines = ["* It's the end.", "* You feel a nearby breeze of dust hit your face.", "* It's fun^1, isn't it?"];
            if (global.hp[1] <= 0)
            {
                global.es_flv = "* YOU WERE USED UP.";
                return global.es_flv;
            }
            if (scr_havechar(4) && global.hp[4] <= 0)
            {
                global.es_flv = "* NOELLE WAS USED UP.";
                return global.es_flv;
            }
        }
        var k = irandom(2);
        if (k == global.es_flv_last)
            k = (k + 1 + irandom(1)) mod 3;
        global.es_flv_last = k;
        global.es_flv = lines[k];
    }
    return global.es_flv;
}

// dialogue entries the mod adds to scr_text (used where a choice needs a follow-up)
function scr_es_text(argument0)
{
    switch (argument0)
    {
        case 24500:
            global.choicemsg[0] = "It's not#weird";
            global.choicemsg[1] = "(Say#nothing)";
            global.choicemsg[2] = " ";
            global.choicemsg[3] = " ";
            scr_speaker("noelle");
            msgsetloc(0, "\\E9* ...that's weird^1, right?/", "es_lib_26");
            msgnextloc("\\C2 ", "es_lib_27");
            return 1;
        case 24501:
            scr_speaker("noelle");
            if (global.choice == 0)
            {
                msgsetloc(0, "\\E6* ..^1. oh./", "es_lib_28");
                msgnextloc("\\E4* Thanks^1, Kris./", "es_lib_29");
                msgnextloc("\\E4* You're..^1. nicer than people say you are./%", "es_lib_30");
            }
            else
            {
                msgsetloc(0, "\\E6* ..^1. yeah./", "es_lib_31");
                msgnextloc("\\E1* You don't have to say anything^1. It's okay./", "es_lib_32");
                msgnextloc("\\E4* It's kind of nice^1, actually./%", "es_lib_33");
            }
            return 1;
        // the Roaring Knight, standing on the track. It never answers.
        case 24510:
            global.choicemsg[0] = "I know it";
            global.choicemsg[1] = "(Say#nothing)";
            global.choicemsg[2] = " ";
            global.choicemsg[3] = " ";
            scr_speaker("noelle");
            msgsetloc(0, "\\E1* ...Kris. That thing.../", "es_rk_1");
            msgnextloc("\\E1* It isn't looking at me./", "es_rk_2");
            msgnextloc("\\E6* It's looking at you./", "es_rk_3");
            msgnextloc("\\E9* ...do you know it?/", "es_rk_4");
            msgnextloc("\\C2 ", "es_rk_5");
            return 1;
        case 24511:
            scr_speaker("noelle");
            if (global.choice == 0)
            {
                msgsetloc(0, "\\E6* ...of course you do./", "es_rk_6");
                msgnextloc("\\E1* Everything weird in this town always knows you^1, doesn't it?/%", "es_rk_7");
            }
            else
            {
                msgsetloc(0, "\\E1* ...that's a yes^1, isn't it./", "es_rk_8");
                msgnextloc("\\E6* You always go quiet when it's a yes./%", "es_rk_9");
            }
            return 1;
        case 24512:
            global.choicemsg[0] = "Stay";
            global.choicemsg[1] = "(Say#nothing)";
            global.choicemsg[2] = " ";
            global.choicemsg[3] = " ";
            scr_speaker("noelle");
            msgsetloc(0, "\\E9* Kris^1, if it fights us.../", "es_rk_10");
            msgnextloc("\\E1* ...will you stay with me?/", "es_rk_11");
            msgnextloc("\\C2 ", "es_rk_12");
            return 1;
        case 24513:
            scr_speaker("noelle");
            if (global.choice == 0)
            {
                msgsetloc(0, "\\E4* ...okay./", "es_rk_13");
                msgnextloc("\\E1* Then I'm not scared./", "es_rk_14");
                msgnextloc("\\E0* ...I'm not./%", "es_rk_15");
            }
            else
            {
                msgsetloc(0, "\\E6* ...right./", "es_rk_16");
                msgnextloc("\\E1* I'll stay anyway./%", "es_rk_17");
            }
            return 1;
    }
    return 0;
}

// Noelle after her first kill: the weird route's sad overworld sprites
function scr_es_noelle_changed()
{
    return scr_es_on() && global.flag[2455] == 1;
}

function scr_es_noelle_sad_sprites()
{
    dsprite = spr_noelle_walk_down_sad_dw;
    rsprite = spr_noelle_walk_right_sad_dw;
    lsprite = spr_noelle_walk_left_sad_dw;
    usprite = spr_noelle_walk_up_dw;
}
