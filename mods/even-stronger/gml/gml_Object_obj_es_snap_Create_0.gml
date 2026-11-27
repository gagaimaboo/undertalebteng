// Kris snaps. A cutscene, not real hits: the world goes red, everyone becomes a black silhouette,
// Kris blinks between them cutting white lines through them, then one giant X. Nothing survives it.
t = 0;
depth = -99999;
red = 0;
killed = 0;
caster = 0;
kpos = 0;          // which enemy Kris is at (-1 = his own spot)
slashes = [];      // [x1, y1, x2, y2, life]
xslash = 0;
// freeze the battle: no more turns from here on
instance_deactivate_object(obj_battlecontroller);
instance_deactivate_object(obj_dbulletcontroller);
