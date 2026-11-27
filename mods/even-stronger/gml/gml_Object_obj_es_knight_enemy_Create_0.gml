// The Roaring Knight, rebuilt for Chapter 2 with Chapter 3's own sprites.
// It never speaks. The fight ends at 50% HP: it flickers, then Kris cuts it down for real.
scr_enemy_object_init();
image_speed = 0;
idlesprite = scr_es_spr("es_rk_idle");
hurtsprite = idlesprite;
sparedsprite = scr_es_spr("es_rk_hurt");
sprite_index = idlesprite;
image_xscale = 2;
image_yscale = 2;
siner2 = 0;
endcon = 0;
endtimer = 0;
kattack = 0;
aetimer = 0;
talkmax = 15;
