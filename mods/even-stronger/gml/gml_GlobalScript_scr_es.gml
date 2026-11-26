// DT Chapter 2: Even Stronger - core rules.
//
// save flags used by the mod (the game saves flags 0-2499, vanilla Ch2 never goes past ~960):
//   2450  first-ability easter egg seen (Pain)
//   2451  Queen rules active (FIGHT works, abilities hit harder)
//   2452  kills this playthrough
//   2453  opening done
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
    if (global.flag[2451] == 1)
        return d;
    if (char == 1)
        return 0;
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
    return (global.flag[2451] == 1) ? 1.35 : 1;
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
    return max(1, round(((((global.battleat[argument0] * 150) / 20) - (global.monsterdf[argument1] * 3)) * 1.25) * scr_es_abilitymul()));
}

function scr_es_cast_after_pain(argument0, argument1, argument2)
{
    global.es_pain_bypass = 1;
    with (argument2)
        scr_spell(argument0, argument1);
    global.es_pain_bypass = 0;
}

// Kris's skills live in his ACT list (like X-Slash on Spamton NEO), on every enemy
function scr_es_addkrisacts()
{
    if (!scr_havechar(1))
        exit;
    for (var i = 0; i < 3; i++)
    {
        if (global.monster[i] != 1)
            continue;
        var hasx = -1;
        var hass = -1;
        for (var k = 0; k < 6; k++)
        {
            if (global.canact[i][k] == 1 && global.actname[i][k] == "X-Slash")
                hasx = k;
            if (global.canact[i][k] == 1 && global.actname[i][k] == "Slashrush")
                hass = k;
        }
        if (hasx < 0)
        {
            for (var k = 0; k < 6; k++)
            {
                if (global.canact[i][k] == 0)
                {
                    global.canact[i][k] = 1;
                    global.actname[i][k] = "X-Slash";
                    global.actdesc[i][k] = "Physical#damage";
                    global.actcost[i][k] = 62;
                    global.actactor[i][k] = 1;
                    global.actsimul[i][k] = 0;
                    break;
                }
            }
        }
        if (hass < 0)
        {
            for (var k = 0; k < 6; k++)
            {
                if (global.canact[i][k] == 0)
                {
                    global.canact[i][k] = 1;
                    global.actname[i][k] = "Slashrush";
                    global.actdesc[i][k] = "Mash Z!";
                    global.actcost[i][k] = 200;
                    global.actactor[i][k] = 1;
                    global.actsimul[i][k] = 0;
                    break;
                }
            }
        }
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
        var k = irandom(2);
        if (k == global.es_flv_last)
            k = (k + 1 + irandom(1)) mod 3;
        global.es_flv_last = k;
        global.es_flv = lines[k];
    }
    return global.es_flv;
}
