// Even Stronger: landing in the Cyber World. It's Noelle next to Kris this time.
con = 1;
scr_losechar();
if (instance_exists(obj_caterpillarchara))
    instance_destroy(obj_caterpillarchara);
scr_getchar(4);
scr_es_setspells();
scr_makecaterpillar(obj_mainchara.x - 40, obj_mainchara.y, 4, 0);
