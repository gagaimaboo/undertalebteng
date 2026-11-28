t++;
switch (phase)
{
    case 0:
        if (t > 10)
        {
            if (left_p() || right_p())
            {
                sel = 1 - sel;
                snd_play(snd_menumove);
            }
            if (button1_p())
            {
                snd_play(snd_select);
                phase = 1;
                t = 0;
            }
        }
        break;
    case 1:
        if ((t > 15 && button1_p()) || t > 75)
        {
            phase = 2;
            t = 0;
            dogx = camerax() + 700;
        }
        break;
    case 2:
        var mid = camerax() + 310;
        if (dogx > mid)
            dogx = max(mid, dogx - 9);
        else if (barked < 2)
        {
            if (t mod 18 == 0)
            {
                snd_play(snd_pombark);
                barked++;
            }
        }
        else if (t > 0)
        {
            phase = 3;
            t = 0;
        }
        break;
    case 3:
        dogx -= 9;
        if (t > 30)
        {
            scr_es_cast_after_pain(spell, caster, hero);
            instance_destroy();
        }
        break;
}
