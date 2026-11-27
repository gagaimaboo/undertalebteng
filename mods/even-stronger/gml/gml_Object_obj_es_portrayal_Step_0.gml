t++;
if (phase > 0)
    a = min(1, a + 0.05);
else
    a = max(0, a - 0.05);
if (phase == 2)
{
    t2++;
    if ((t2 mod 3) == 0 && loff < ln)
    {
        lon[lorder[loff]] = 0;
        loff++;
        if ((loff mod 3) == 1)
            snd_pitch(snd_play(snd_icespell), 1.6 + random(0.4));
    }
}
for (var i = 0; i < 60; i++)
{
    sy[i] += ss[i];
    sx[i] += sin((t + i * 13) / 30) * 0.4;
    if (sy[i] > 260)
        sy[i] -= 260;
}
