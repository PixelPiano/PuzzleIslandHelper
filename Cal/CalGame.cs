using Celeste.Mod.Entities;
using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Cal
{
    public class CalGame
    {
        public class Cloud
        {
            public float x;

            public float y;

            public float spd;

            public float w;
        }
        public class Particle
        {
            public float x;

            public float y;

            public int s;

            public float spd;

            public float off;

            public int c;
        }
        public class DeadParticle
        {
            public float x;

            public float y;

            public int t;

            public Vector2 spd;
        }
        public class player : GameObject
        {
            public bool p_jump;

            public bool p_dash;

            public int grace;

            public int jbuffer;

            public int djump;

            public int dash_time;

            public int dash_effect_time;

            public Vector2 dash_target = new Vector2(0f, 0f);

            public Vector2 dash_accel = new Vector2(0f, 0f);

            public float spr_off;

            public bool was_on_ground;

            public player_hair hair;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                spr = 1f;
                djump = g.max_djump;
                hitbox = new Rectangle(1, 3, 6, 5);
            }

            
            public override void update()
            {
                if (G.pause_player)
                {
                    return;
                }

                int num = (E.btn(G.k_right) ? 1 : (E.btn(G.k_left) ? (-1) : 0));
                if (G.spikes_at(x + (float)hitbox.X, y + (float)hitbox.Y, hitbox.Width, hitbox.Height, spd.X, spd.Y))
                {
                    G.kill_player(this);
                }

                if (y > 128f)
                {
                    G.kill_player(this);
                }

                bool flag = is_solid(0, 1);
                bool flag2 = is_ice(0, 1);
                if (flag && !was_on_ground)
                {
                    G.init_object(new smoke(), x, y + 4f);
                }

                bool num2 = E.btn(G.k_jump) && !p_jump;
                p_jump = E.btn(G.k_jump);
                if (num2)
                {
                    jbuffer = 4;
                }
                else if (jbuffer > 0)
                {
                    jbuffer--;
                }

                bool flag3 = E.btn(G.k_dash) && !p_dash;
                p_dash = E.btn(G.k_dash);
                if (flag)
                {
                    grace = 6;
                    if (djump < G.max_djump)
                    {
                        G.psfx(54);
                        djump = G.max_djump;
                    }
                }
                else if (grace > 0)
                {
                    grace--;
                }

                dash_effect_time--;
                if (dash_time > 0)
                {
                    G.init_object(new smoke(), x, y);
                    dash_time--;
                    spd.X = G.appr(spd.X, dash_target.X, dash_accel.X);
                    spd.Y = G.appr(spd.Y, dash_target.Y, dash_accel.Y);
                }
                else
                {
                    int num3 = 1;
                    float amount = 0.6f;
                    float amount2 = 0.15f;
                    if (!flag)
                    {
                        amount = 0.4f;
                    }
                    else if (flag2)
                    {
                        amount = 0.05f;
                        if (num == ((!flipX) ? 1 : (-1)))
                        {
                            amount = 0.05f;
                        }
                    }

                    if (E.abs(spd.X) > (float)num3)
                    {
                        spd.X = G.appr(spd.X, E.sign(spd.X) * num3, amount2);
                    }
                    else
                    {
                        spd.X = G.appr(spd.X, num * num3, amount);
                    }

                    if (spd.X != 0f)
                    {
                        flipX = spd.X < 0f;
                    }

                    float target = 2f;
                    float num4 = 0.21f;
                    if (E.abs(spd.Y) <= 0.15f)
                    {
                        num4 *= 0.5f;
                    }

                    if (num != 0 && is_solid(num, 0) && !is_ice(num, 0))
                    {
                        target = 0.4f;
                        if (E.rnd(10f) < 2f)
                        {
                            G.init_object(new smoke(), x + (float)(num * 6), y);
                        }
                    }

                    if (!flag)
                    {
                        spd.Y = G.appr(spd.Y, target, num4);
                    }

                    if (jbuffer > 0)
                    {
                        if (grace > 0)
                        {
                            G.psfx(1);
                            jbuffer = 0;
                            grace = 0;
                            spd.Y = -2f;
                            G.init_object(new smoke(), x, y + 4f);
                        }
                        else
                        {
                            int num5 = (is_solid(-3, 0) ? (-1) : (is_solid(3, 0) ? 1 : 0));
                            if (num5 != 0)
                            {
                                G.psfx(2);
                                jbuffer = 0;
                                spd.Y = -2f;
                                spd.X = -num5 * (num3 + 1);
                                if (!is_ice(num5 * 3, 0))
                                {
                                    G.init_object(new smoke(), x + (float)(num5 * 6), y);
                                }
                            }
                        }
                    }

                    int num6 = 5;
                    float num7 = (float)num6 * 0.707106769f;
                    if (djump > 0 && flag3)
                    {
                        G.init_object(new smoke(), x, y);
                        djump--;
                        dash_time = 4;
                        G.has_dashed = true;
                        dash_effect_time = 10;
                        int num8 = E.dashDirectionX((!flipX) ? 1 : (-1));
                        int num9 = E.dashDirectionY((!flipX) ? 1 : (-1));
                        if (num8 != 0 && num9 != 0)
                        {
                            spd.X = (float)num8 * num7;
                            spd.Y = (float)num9 * num7;
                        }
                        else if (num8 != 0)
                        {
                            spd.X = num8 * num6;
                            spd.Y = 0f;
                        }
                        else if (num9 != 0)
                        {
                            spd.X = 0f;
                            spd.Y = num9 * num6;
                        }
                        else
                        {
                            spd.X = ((!flipX) ? 1 : (-1));
                            spd.Y = 0f;
                        }

                        G.psfx(3);
                        G.freeze = 2;
                        G.shake = 6;
                        dash_target.X = 2 * E.sign(spd.X);
                        dash_target.Y = 2 * E.sign(spd.Y);
                        dash_accel.X = 1.5f;
                        dash_accel.Y = 1.5f;
                        if (spd.Y < 0f)
                        {
                            dash_target.Y *= 0.75f;
                        }

                        if (spd.Y != 0f)
                        {
                            dash_accel.X *= 0.707106769f;
                        }

                        if (spd.X != 0f)
                        {
                            dash_accel.Y *= 0.707106769f;
                        }
                    }
                    else if (flag3 && djump <= 0)
                    {
                        G.psfx(9);
                        G.init_object(new smoke(), x, y);
                    }
                }

                spr_off += 0.25f;
                if (!flag)
                {
                    if (is_solid(num, 0))
                    {
                        spr = 5f;
                    }
                    else
                    {
                        spr = 3f;
                    }
                }
                else if (E.btn(G.k_down))
                {
                    spr = 6f;
                }
                else if (E.btn(G.k_up))
                {
                    spr = 7f;
                }
                else if (spd.X == 0f || (!E.btn(G.k_left) && !E.btn(G.k_right)))
                {
                    spr = 1f;
                }
                else
                {
                    spr = 1f + spr_off % 4f;
                }

                if (y < -4f && G.level_index() < 30)
                {
                    G.next_room();
                }

                was_on_ground = flag;
            }

            
            public override void draw()
            {
                if (x < -1f || x > 121f)
                {
                    x = G.clamp(x, -1f, 121f);
                    spd.X = 0f;
                }

                hair.draw_hair(this, (!flipX) ? 1 : (-1), djump);
                G.draw_player(this, djump);
            }

            
            public player()
            {
            }
        }

        public class player_hair
        {
            
            public class node
            {
                public float x;

                public float y;

                public float size;
            }

            
            public node[] hair = new node[5];

            
            public CalEmu E;

            
            public CalGame G;

            
            public player_hair(GameObject obj)
            {
                E = obj.E;
                G = obj.G;
                for (int i = 0; i <= 4; i++)
                {
                    hair[i] = new node
                    {
                        x = obj.x,
                        y = obj.y,
                        size = E.max(1f, E.min(2f, 3 - i))
                    };
                }
            }

            
            public void draw_hair(GameObject obj, int facing, int djump)
            {
                int num = djump switch
                {
                    2 => 7 + E.flr(G.frames / 3 % 2) * 4,
                    1 => 8,
                    _ => 12,
                };
                Vector2 vector = new Vector2(obj.x + 4f - (float)(facing * 2), obj.y + (float)(E.btn(G.k_down) ? 4 : 3));
                node[] array = hair;
                foreach (node node in array)
                {
                    node.x += (vector.X - node.x) / 1.5f;
                    node.y += (vector.Y + 0.5f - node.y) / 1.5f;
                    E.circfill(node.x, node.y, node.size, num);
                    vector = new Vector2(node.x, node.y);
                }
            }
        }

        public class player_spawn : GameObject
        {
            
            public Vector2 target;

            
            public int state;

            
            public int delay;

            
            public player_hair hair;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                spr = 3f;
                target = new Vector2(x, y);
                y = 128f;
                spd.Y = -4f;
                state = 0;
                delay = 0;
                solids = false;
                hair = new player_hair(this);
                E.sfx(4);
            }

            
            public override void update()
            {
                if (state == 0)
                {
                    if (y < target.Y + 16f)
                    {
                        state = 1;
                        delay = 3;
                    }
                }
                else if (state == 1)
                {
                    spd.Y += 0.5f;
                    if (spd.Y > 0f && delay > 0)
                    {
                        spd.Y = 0f;
                        delay--;
                    }

                    if (spd.Y > 0f && y > target.Y)
                    {
                        y = target.Y;
                        spd = new Vector2(0f, 0f);
                        state = 2;
                        delay = 5;
                        G.shake = 5;
                        G.init_object(new smoke(), x, y + 4f);
                        E.sfx(5);
                    }
                }
                else if (state == 2)
                {
                    delay--;
                    spr = 6f;
                    if (delay < 0)
                    {
                        G.destroy_object(this);
                        G.init_object(new player(), x, y).hair = hair;
                    }
                }
            }

            
            public override void draw()
            {
                hair.draw_hair(this, 1, G.max_djump);
                G.draw_player(this, G.max_djump);
            }
        }

        public class spring : GameObject
        {
            public int hide_in;

            
            public int hide_for;

            
            public int delay;

            
            public override void update()
            {
                if (hide_for > 0)
                {
                    hide_for--;
                    if (hide_for <= 0)
                    {
                        spr = 18f;
                        delay = 0;
                    }
                }
                else if (spr == 18f)
                {
                    player player = collide<player>(0, 0);
                    if (player != null && player.spd.Y >= 0f)
                    {
                        spr = 19f;
                        player.y = y - 4f;
                        player.spd.X *= 0.2f;
                        player.spd.Y = -3f;
                        player.djump = G.max_djump;
                        delay = 10;
                        G.init_object(new smoke(), x, y);
                        fall_floor fall_floor = collide<fall_floor>(0, 1);
                        if (fall_floor != null)
                        {
                            G.break_fall_floor(fall_floor);
                        }

                        G.psfx(8);
                    }
                }
                else if (delay > 0)
                {
                    delay--;
                    if (delay <= 0)
                    {
                        spr = 18f;
                    }
                }

                if (hide_in > 0)
                {
                    hide_in--;
                    if (hide_in <= 0)
                    {
                        hide_for = 60;
                        spr = 0f;
                    }
                }
            }
        }

        public class balloon : GameObject
        {
            
            public float offset;

            
            public float start;

            
            public float timer;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                offset = E.rnd(1f);
                start = y;
                hitbox = new Rectangle(-1, -1, 10, 10);
            }

            
            public override void update()
            {
                if (spr == 22f)
                {
                    offset += 0.01f;
                    y = start + E.sin(offset) * 2f;
                    player player = collide<player>(0, 0);
                    if (player != null && player.djump < G.max_djump)
                    {
                        G.psfx(6);
                        G.init_object(new smoke(), x, y);
                        player.djump = G.max_djump;
                        spr = 0f;
                        timer = 60f;
                    }
                }
                else if (timer > 0f)
                {
                    timer -= 1f;
                }
                else
                {
                    G.psfx(7);
                    G.init_object(new smoke(), x, y);
                    spr = 22f;
                }
            }

            
            public override void draw()
            {
                if (spr == 22f)
                {
                    E.spr(13f + offset * 8f % 3f, x, y + 6f);
                    E.spr(spr, x, y);
                }
            }
        }

        public class fall_floor : GameObject
        {
            public int state;

            public bool solid = true;

            public int delay;

            
            public override void update()
            {
                if (state == 0)
                {
                    if (check<player>(0, -1) || check<player>(-1, 0) || check<player>(1, 0))
                    {
                        G.break_fall_floor(this);
                    }
                }
                else if (state == 1)
                {
                    delay--;
                    if (delay <= 0)
                    {
                        state = 2;
                        delay = 60;
                        collideable = false;
                    }
                }
                else if (state == 2)
                {
                    delay--;
                    if (delay <= 0 && !check<player>(0, 0))
                    {
                        G.psfx(7);
                        state = 0;
                        collideable = true;
                        G.init_object(new smoke(), x, y);
                    }
                }
            }

            
            public override void draw()
            {
                if (state != 2)
                {
                    if (state != 1)
                    {
                        E.spr(23f, x, y);
                    }
                    else
                    {
                        E.spr(23 + (15 - delay) / 5, x, y);
                    }
                }
            }
        }

        public class smoke : GameObject
        {
            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                spr = 29f;
                spd.Y = -0.1f;
                spd.X = 0.3f + E.rnd(0.2f);
                x += -1f + E.rnd(2f);
                y += -1f + E.rnd(2f);
                flipX = G.maybe();
                flipY = G.maybe();
                solids = false;
            }

            
            public override void update()
            {
                spr += 0.2f;
                if (spr >= 32f)
                {
                    G.destroy_object(this);
                }
            }
        }

        public class fruit : GameObject
        {
            
            public float start;

            
            public float off;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                spr = 26f;
                start = y;
                off = 0f;
            }

            
            public override void update()
            {
                player player = collide<player>(0, 0);
                if (player != null)
                {
                    player.djump = G.max_djump;
                    G.sfx_timer = 20;
                    E.sfx(13);
                    G.got_fruit.Add(1 + G.level_index());
                    G.init_object(new lifeup(), x, y);
                    G.destroy_object(this);
                    Stats.Increment(Stat.PICO_BERRIES);
                }

                off += 1f;
                y = start + E.sin(off / 40f) * 2.5f;
            }
        }

        public class fly_fruit : GameObject
        {
            
            public float start;

            
            public bool fly;

            
            public float step = 0.5f;

            
            public float sfx_delay = 8f;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                start = y;
                solids = false;
            }

            
            public override void update()
            {
                if (fly)
                {
                    if (sfx_delay > 0f)
                    {
                        sfx_delay -= 1f;
                        if (sfx_delay <= 0f)
                        {
                            G.sfx_timer = 20;
                            E.sfx(14);
                        }
                    }

                    spd.Y = G.appr(spd.Y, -3.5f, 0.25f);
                    if (y < -16f)
                    {
                        G.destroy_object(this);
                    }
                }
                else
                {
                    if (G.has_dashed)
                    {
                        fly = true;
                    }

                    step += 0.05f;
                    spd.Y = E.sin(step) * 0.5f;
                }

                player player = collide<player>(0, 0);
                if (player != null)
                {
                    player.djump = G.max_djump;
                    G.sfx_timer = 20;
                    E.sfx(13);
                    G.got_fruit.Add(1 + G.level_index());
                    G.init_object(new lifeup(), x, y);
                    G.destroy_object(this);
                    Stats.Increment(Stat.PICO_BERRIES);
                }
            }

            
            public override void draw()
            {
                float num = 0f;
                if (!fly)
                {
                    if (E.sin(step) < 0f)
                    {
                        num = 1f + E.max(0f, G.sign(y - start));
                    }
                }
                else
                {
                    num = (num + 0.25f) % 3f;
                }

                E.spr(45f + num, x - 6f, y - 2f, 1, 1, flipX: true);
                E.spr(spr, x, y);
                E.spr(45f + num, x + 6f, y - 2f);
            }

            
            public fly_fruit()
            {
            }
        }

        public class lifeup : GameObject
        {
            
            public int duration;

            
            public float flash;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                spd.Y = -0.25f;
                duration = 30;
                x -= 2f;
                y -= 4f;
                flash = 0f;
                solids = false;
            }

            
            public override void update()
            {
                duration--;
                if (duration <= 0)
                {
                    G.destroy_object(this);
                }
            }

            
            public override void draw()
            {
                flash += 0.5f;
                E.print("1000", x - 2f, y, 7f + flash % 2f);
            }
        }

        public class fake_wall : GameObject
        {
            
            public override void update()
            {
                hitbox = new Rectangle(-1, -1, 18, 18);
                player player = collide<player>(0, 0);
                if (player != null && player.dash_effect_time > 0)
                {
                    player.spd.X = (float)(-G.sign(player.spd.X)) * 1.5f;
                    player.spd.Y = -1.5f;
                    player.dash_time = -1;
                    G.sfx_timer = 20;
                    E.sfx(16);
                    G.destroy_object(this);
                    G.init_object(new smoke(), x, y);
                    G.init_object(new smoke(), x + 8f, y);
                    G.init_object(new smoke(), x, y + 8f);
                    G.init_object(new smoke(), x + 8f, y + 8f);
                    G.init_object(new fruit(), x + 4f, y + 4f);
                }

                hitbox = new Rectangle(0, 0, 16, 16);
            }

            
            public override void draw()
            {
                E.spr(64f, x, y);
                E.spr(65f, x + 8f, y);
                E.spr(80f, x, y + 8f);
                E.spr(81f, x + 8f, y + 8f);
            }
        }

        public class key : GameObject
        {
            
            public override void update()
            {
                int num = E.flr(spr);
                spr = 9f + (E.sin((float)G.frames / 30f) + 0.5f) * 1f;
                int num2 = E.flr(spr);
                if (num2 == 10 && num2 != num)
                {
                    flipX = !flipX;
                }

                if (check<player>(0, 0))
                {
                    E.sfx(23);
                    G.sfx_timer = 20;
                    G.destroy_object(this);
                    G.has_key = true;
                }
            }
        }

        public class chest : GameObject
        {
            
            public float start;

            
            public float timer;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                x -= 4f;
                start = x;
                timer = 20f;
            }

            
            public override void update()
            {
                if (G.has_key)
                {
                    timer -= 1f;
                    x = start - 1f + E.rnd(3f);
                    if (timer <= 0f)
                    {
                        G.sfx_timer = 20;
                        E.sfx(16);
                        G.init_object(new fruit(), x, y - 4f);
                        G.destroy_object(this);
                    }
                }
            }
        }

        public class platform : GameObject
        {
            public float dir;

            
            public float last;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                x -= 4f;
                solids = false;
                hitbox.Width = 16;
                last = x;
            }

            
            public override void update()
            {
                spd.X = dir * 0.65f;
                if (x < -16f)
                {
                    x = 128f;
                }

                if (x > 128f)
                {
                    x = -16f;
                }

                if (!check<player>(0, 0))
                {
                    collide<player>(0, -1)?.move_x((int)(x - last), 1);
                }

                last = x;
            }

            
            public override void draw()
            {
                E.spr(11f, x, y - 1f);
                E.spr(12f, x + 8f, y - 1f);
            }
        }

        public class message : GameObject
        {
            
            public float last;

            
            public float index;

            
            public override void draw()
            {
                string text = "-- celeste mountain --#this memorial to those# perished on the climb";
                if (check<player>(4, 0))
                {
                    if (index < (float)text.Length)
                    {
                        index += 0.5f;
                        if (index >= last + 1f)
                        {
                            last += 1f;
                            E.sfx(35);
                        }
                    }

                    Vector2 vector = new Vector2(8f, 96f);
                    for (int i = 0; (float)i < index; i++)
                    {
                        if (text[i] != '#')
                        {
                            E.rectfill(vector.X - 2f, vector.Y - 2f, vector.X + 7f, vector.Y + 6f, 7f);
                            E.print(text[i].ToString() ?? "", vector.X, vector.Y, 0f);
                            vector.X += 5f;
                        }
                        else
                        {
                            vector.X = 8f;
                            vector.Y += 7f;
                        }
                    }
                }
                else
                {
                    index = 0f;
                    last = 0f;
                }
            }
        }

        public class big_chest : GameObject
        {
            
            public class particle
            {
                public float x;

                public float y;

                public float h;

                public float spd;
            }

            
            public int state;

            
            public float timer;

            
            public List<particle> particles;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                hitbox.Width = 16;
            }

            
            public override void draw()
            {
                if (state == 0)
                {
                    player player = collide<player>(0, 8);
                    if (player != null && player.is_solid(0, 1))
                    {
                        E.music(-1, 500, 7);
                        E.sfx(37);
                        G.pause_player = true;
                        player.spd.X = 0f;
                        player.spd.Y = 0f;
                        state = 1;
                        G.init_object(new smoke(), x, y);
                        G.init_object(new smoke(), x + 8f, y);
                        timer = 60f;
                        particles = new List<particle>();
                    }

                    E.spr(96f, x, y);
                    E.spr(97f, x + 8f, y);
                }
                else if (state == 1)
                {
                    timer -= 1f;
                    G.shake = 5;
                    G.flash_bg = true;
                    if (timer <= 45f && particles.Count < 50)
                    {
                        particles.Add(new particle
                        {
                            x = 1f + E.rnd(14f),
                            y = 0f,
                            h = 32f + E.rnd(32f),
                            spd = 8f + E.rnd(8f)
                        });
                    }

                    if (timer < 0f)
                    {
                        state = 2;
                        particles.Clear();
                        G.flash_bg = false;
                        G.new_bg = true;
                        G.init_object(new orb(), x + 4f, y + 4f);
                        G.pause_player = false;
                    }

                    foreach (particle particle in particles)
                    {
                        particle.y += particle.spd;
                        E.rectfill(x + particle.x, y + 8f - particle.y, x + particle.x, E.min(y + 8f - particle.y + particle.h, y + 8f), 7f);
                    }
                }

                E.spr(112f, x, y + 8f);
                E.spr(113f, x + 8f, y + 8f);
            }
        }

        public class orb : GameObject
        {
            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                spd.Y = -4f;
                solids = false;
            }

            
            public override void draw()
            {
                spd.Y = G.appr(spd.Y, 0f, 0.5f);
                player player = collide<player>(0, 0);
                if (spd.Y == 0f && player != null)
                {
                    G.music_timer = 45;
                    E.sfx(51);
                    G.freeze = 10;
                    G.shake = 10;
                    G.destroy_object(this);
                    G.max_djump = 2;
                    player.djump = 2;
                }

                E.spr(102f, x, y);
                float num = (float)G.frames / 30f;
                for (int i = 0; i <= 7; i++)
                {
                    E.circfill(x + 4f + E.cos(num + (float)i / 8f) * 8f, y + 4f + E.sin(num + (float)i / 8f) * 8f, 1f, 7f);
                }
            }
        }

        public class flag : GameObject
        {
            
            public float score;

            
            public bool show;

            
            public override void init(CalGame g, CalEmu e)
            {
                base.init(g, e);
                x += 5f;
                score = G.got_fruit.Count;
                Stats.Increment(Stat.PICO_COMPLETES);
                Achievements.Register(Achievement.PICO8);
            }

            
            public override void draw()
            {
                spr = 118f + (float)G.frames / 5f % 3f;
                E.spr(spr, x, y);
                if (show)
                {
                    E.rectfill(32f, 2f, 96f, 31f, 0f);
                    E.spr(26f, 55f, 6f);
                    E.print("x" + score, 64f, 9f, 7f);
                    G.draw_time(49, 16);
                    E.print("deaths:" + G.deaths, 48f, 24f, 7f);
                }
                else if (check<player>(0, 0))
                {
                    E.sfx(55);
                    G.sfx_timer = 30;
                    show = true;
                }
            }
        }

        public class room_title : GameObject
        {
            
            public float delay = 5f;

            
            public override void draw()
            {
                delay -= 1f;
                if (delay < -30f)
                {
                    G.destroy_object(this);
                }
                else if (delay < 0f)
                {
                    E.rectfill(24f, 58f, 104f, 70f, 0f);
                    if (G.room.X == 3 && G.room.Y == 1)
                    {
                        E.print("old site", 48f, 62f, 7f);
                    }
                    else if (G.level_index() == 30)
                    {
                        E.print("summit", 52f, 62f, 7f);
                    }
                    else
                    {
                        int num = (1 + G.level_index()) * 100;
                        E.print(num + "m", 52 + ((num < 1000) ? 2 : 0), 62f, 7f);
                    }

                    G.draw_time(4, 4);
                }
            }
        }

        public class GameObject
        {
            public CalGame G;

            public CalEmu E;

            public int type;

            public bool collideable = true;

            public bool solids = true;

            public float spr;

            public bool flipX;

            public bool flipY;

            public float x;

            public float y;

            public Rectangle hitbox = new Rectangle(0, 0, 8, 8);

            public Vector2 spd = new Vector2(0f, 0f);

            public Vector2 rem = new Vector2(0f, 0f);

            public virtual void init(CalGame g, CalEmu e)
            {
                G = g;
                E = e;
            }

            public virtual void update()
            {
            }

            
            public virtual void draw()
            {
                if (spr > 0f)
                {
                    E.spr(spr, x, y, 1, 1, flipX, flipY);
                }
            }

            
            public bool is_solid(int ox, int oy)
            {
                if (oy > 0 && !check<platform>(ox, 0) && check<platform>(ox, oy))
                {
                    return true;
                }

                if (!G.solid_at(x + (float)hitbox.X + (float)ox, y + (float)hitbox.Y + (float)oy, hitbox.Width, hitbox.Height) && !check<fall_floor>(ox, oy))
                {
                    return check<fake_wall>(ox, oy);
                }

                return true;
            }

            
            public bool is_ice(int ox, int oy)
            {
                return G.ice_at(x + (float)hitbox.X + (float)ox, y + (float)hitbox.Y + (float)oy, hitbox.Width, hitbox.Height);
            }

            
            public T collide<T>(int ox, int oy) where T : GameObject
            {
                Type typeFromHandle = typeof(T);
                foreach (GameObject @object in G.objects)
                {
                    if (@object != null && @object.GetType() == typeFromHandle && @object != this && @object.collideable && @object.x + (float)@object.hitbox.X + (float)@object.hitbox.Width > x + (float)hitbox.X + (float)ox && @object.y + (float)@object.hitbox.Y + (float)@object.hitbox.Height > y + (float)hitbox.Y + (float)oy && @object.x + (float)@object.hitbox.X < x + (float)hitbox.X + (float)hitbox.Width + (float)ox && @object.y + (float)@object.hitbox.Y < y + (float)hitbox.Y + (float)hitbox.Height + (float)oy)
                    {
                        return @object as T;
                    }
                }

                return null;
            }

            public bool check<T>(int ox, int oy) where T : GameObject
            {
                return collide<T>(ox, oy) != null;
            }

            
            public void move(float ox, float oy)
            {
                int num = 0;
                rem.X += ox;
                num = E.flr(rem.X + 0.5f);
                rem.X -= num;
                move_x(num, 0);
                rem.Y += oy;
                num = E.flr(rem.Y + 0.5f);
                rem.Y -= num;
                move_y(num);
            }

            
            public void move_x(int amount, int start)
            {
                if (solids)
                {
                    int num = G.sign(amount);
                    for (int i = start; (float)i <= E.abs(amount); i++)
                    {
                        if (!is_solid(num, 0))
                        {
                            x += num;
                            continue;
                        }

                        spd.X = 0f;
                        rem.X = 0f;
                        break;
                    }
                }
                else
                {
                    x += amount;
                }
            }

            
            public void move_y(int amount)
            {
                if (solids)
                {
                    int num = G.sign(amount);
                    for (int i = 0; (float)i <= E.abs(amount); i++)
                    {
                        if (!is_solid(0, num))
                        {
                            y += num;
                            continue;
                        }

                        spd.Y = 0f;
                        rem.Y = 0f;
                        break;
                    }
                }
                else
                {
                    y += amount;
                }
            }

            
            public GameObject()
            {
            }
        }

        public CalEmu E;

        
        public Point room;

        
        public List<GameObject> objects;

        public int freeze;

        
        public int shake;

        
        public bool will_restart;

        
        public int delay_restart;

        
        public HashSet<int> got_fruit;

        
        public bool has_dashed;

        
        public int sfx_timer;

        
        public bool has_key;

        
        public bool pause_player;

        
        public bool flash_bg;

        
        public int music_timer;

        
        public bool new_bg;

        
        public int k_left;

        
        public int k_right = 1;

        
        public int k_up = 2;

        
        public int k_down = 3;

        
        public int k_jump = 4;

        
        public int k_dash = 5;

        
        public int frames;

        
        public int seconds;

        
        public int minutes;

        
        public int deaths;

        
        public int max_djump;

        
        public bool start_game;

        
        public int start_game_flash;

        
        public bool room_just_loaded;

        
        public List<Cloud> clouds;

        
        public List<Particle> particles;

        
        public List<DeadParticle> dead_particles;

        
        public void Init(CalEmu emulator)
        {
            E = emulator;
            room = new Point(0, 0);
            objects = new List<GameObject>();
            freeze = 0;
            will_restart = false;
            delay_restart = 0;
            got_fruit = new HashSet<int>();
            has_dashed = false;
            sfx_timer = 0;
            has_key = false;
            pause_player = false;
            flash_bg = false;
            music_timer = 0;
            new_bg = false;
            room_just_loaded = false;
            frames = 0;
            seconds = 0;
            minutes = 0;
            deaths = 0;
            max_djump = 1;
            start_game = false;
            start_game_flash = 0;
            clouds = new List<Cloud>();
            for (int i = 0; i <= 16; i++)
            {
                clouds.Add(new Cloud
                {
                    x = E.rnd(128f),
                    y = E.rnd(128f),
                    spd = 1f + E.rnd(4f),
                    w = 32f + E.rnd(32f)
                });
            }

            particles = new List<Particle>();
            for (int j = 0; j <= 32; j++)
            {
                particles.Add(new Particle
                {
                    x = E.rnd(128f),
                    y = E.rnd(128f),
                    s = E.flr(E.rnd(5f) / 4f),
                    spd = 0.25f + E.rnd(5f),
                    off = E.rnd(1f),
                    c = 6 + E.flr(0.5f + E.rnd(1f))
                });
            }

            dead_particles = new List<DeadParticle>();
            title_screen();
        }

        
        
        public void title_screen()
        {
            got_fruit = new HashSet<int>();
            frames = 0;
            deaths = 0;
            max_djump = 1;
            start_game = false;
            start_game_flash = 0;
            E.music(40, 0, 7);
            load_room(7, 3);
        }

        
        
        public void begin_game()
        {
            frames = 0;
            seconds = 0;
            minutes = 0;
            music_timer = 0;
            start_game = false;
            E.music(0, 0, 7);
            load_room(0, 0);
        }

        
        
        public int level_index()
        {
            return room.X % 8 + room.Y * 8;
        }

        
        public bool is_title()
        {
            return level_index() == 31;
        }

        
        
        public void psfx(int num)
        {
            if (sfx_timer <= 0)
            {
                E.sfx(num);
            }
        }

        
        
        public void draw_player(GameObject obj, int djump)
        {
            int num = 0;
            switch (djump)
            {
                case 2:
                    num = ((E.flr(frames / 3 % 2) != 0) ? 144 : 160);
                    break;
                case 0:
                    num = 128;
                    break;
            }

            E.spr(obj.spr + (float)num, obj.x, obj.y, 1, 1, obj.flipX, obj.flipY);
        }

        
        public void break_spring(spring obj)
        {
            obj.hide_in = 15;
        }

        
        
        public void break_fall_floor(fall_floor obj)
        {
            if (obj.state == 0)
            {
                psfx(15);
                obj.state = 1;
                obj.delay = 15;
                init_object(new smoke(), obj.x, obj.y);
                spring spring = obj.collide<spring>(0, -1);
                if (spring != null)
                {
                    break_spring(spring);
                }
            }
        }

        
        
        public T init_object<T>(T obj, float x, float y, int? tile = null) where T : GameObject
        {
            objects.Add(obj);
            if (tile.HasValue)
            {
                obj.spr = tile.Value;
            }

            obj.x = (int)x;
            obj.y = (int)y;
            obj.init(this, E);
            return obj;
        }

        
        
        public void destroy_object(GameObject obj)
        {
            int num = objects.IndexOf(obj);
            if (num >= 0)
            {
                objects[num] = null;
            }
        }

        
        
        public void kill_player(player obj)
        {
            sfx_timer = 12;
            E.sfx(0);
            deaths++;
            shake = 10;
            destroy_object(obj);
            Stats.Increment(Stat.PICO_DEATHS);
            dead_particles.Clear();
            for (int i = 0; i <= 7; i++)
            {
                float num = (float)i / 8f;
                dead_particles.Add(new DeadParticle
                {
                    x = obj.x + 4f,
                    y = obj.y + 4f,
                    t = 10,
                    spd = new Vector2(E.cos(num) * 3f, E.sin(num + 0.5f) * 3f)
                });
            }

            restart_room();
        }

        
        public void restart_room()
        {
            will_restart = true;
            delay_restart = 15;
        }

        
        
        public void next_room()
        {
            if (room.X == 2 && room.Y == 1)
            {
                E.music(30, 500, 7);
            }
            else if (room.X == 3 && room.Y == 1)
            {
                E.music(20, 500, 7);
            }
            else if (room.X == 4 && room.Y == 2)
            {
                E.music(30, 500, 7);
            }
            else if (room.X == 5 && room.Y == 3)
            {
                E.music(30, 500, 7);
            }

            if (room.X == 7)
            {
                load_room(0, room.Y + 1);
            }
            else
            {
                load_room(room.X + 1, room.Y);
            }
        }

        
        public void load_room(int x, int y)
        {
            room_just_loaded = true;
            has_dashed = false;
            has_key = false;
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i] = null;
            }

            room.X = x;
            room.Y = y;
            for (int j = 0; j <= 15; j++)
            {
                for (int k = 0; k <= 15; k++)
                {
                    int num = E.mget(room.X * 16 + j, room.Y * 16 + k);
                    switch (num)
                    {
                        case 11:
                            init_object(new platform(), j * 8, k * 8).dir = -1f;
                            continue;
                        case 12:
                            init_object(new platform(), j * 8, k * 8).dir = 1f;
                            continue;
                    }

                    GameObject classicObject = null;
                    switch (num)
                    {
                        case 1:
                            classicObject = new player_spawn();
                            break;
                        case 18:
                            classicObject = new spring();
                            break;
                        case 22:
                            classicObject = new balloon();
                            break;
                        case 23:
                            classicObject = new fall_floor();
                            break;
                        case 86:
                            classicObject = new message();
                            break;
                        case 96:
                            classicObject = new big_chest();
                            break;
                        case 118:
                            classicObject = new flag();
                            break;
                        default:
                            if (!got_fruit.Contains(1 + level_index()))
                            {
                                switch (num)
                                {
                                    case 26:
                                        classicObject = new fruit();
                                        break;
                                    case 28:
                                        classicObject = new fly_fruit();
                                        break;
                                    case 64:
                                        classicObject = new fake_wall();
                                        break;
                                    case 8:
                                        classicObject = new key();
                                        break;
                                    case 20:
                                        classicObject = new chest();
                                        break;
                                }
                            }

                            break;
                    }

                    if (classicObject != null)
                    {
                        init_object(classicObject, j * 8, k * 8, num);
                    }
                }
            }

            if (!is_title())
            {
                init_object(new room_title(), 0f, 0f);
            }
        }

        
        public void Update()
        {
            frames = (frames + 1) % 30;
            if (frames == 0 && level_index() < 30)
            {
                seconds = (seconds + 1) % 60;
                if (seconds == 0)
                {
                    minutes++;
                }
            }

            if (music_timer > 0)
            {
                music_timer--;
                if (music_timer <= 0)
                {
                    E.music(10, 0, 7);
                }
            }

            if (sfx_timer > 0)
            {
                sfx_timer--;
            }

            if (freeze > 0)
            {
                freeze--;
                return;
            }

            if (shake > 0 && Settings.Instance.ScreenShake != 0)
            {
                shake--;
                E.camera();
                if (shake > 0)
                {
                    if (Settings.Instance.ScreenShake == ScreenshakeAmount.On)
                    {
                        E.camera(-2f + E.rnd(5f), -2f + E.rnd(5f));
                    }
                    else
                    {
                        E.camera(-1f + E.rnd(3f), -1f + E.rnd(3f));
                    }
                }
            }

            if (will_restart && delay_restart > 0)
            {
                delay_restart--;
                if (delay_restart <= 0)
                {
                    will_restart = true;
                    load_room(room.X, room.Y);
                }
            }

            room_just_loaded = false;
            int num = 0;
            while (num != -1)
            {
                int i = num;
                num = -1;
                for (; i < objects.Count; i++)
                {
                    GameObject classicObject = objects[i];
                    if (classicObject != null)
                    {
                        classicObject.move(classicObject.spd.X, classicObject.spd.Y);
                        classicObject.update();
                        if (room_just_loaded)
                        {
                            room_just_loaded = false;
                            num = i;
                            break;
                        }
                    }
                }

                while (objects.IndexOf(null) >= 0)
                {
                    objects.Remove(null);
                }
            }

            if (!is_title())
            {
                return;
            }

            if (!start_game && (E.btn(k_jump) || E.btn(k_dash)))
            {
                E.music(-1, 0, 0);
                start_game_flash = 50;
                start_game = true;
                E.sfx(38);
            }

            if (start_game)
            {
                start_game_flash--;
                if (start_game_flash <= -30)
                {
                    begin_game();
                }
            }
        }

        
        public void Draw()
        {
            E.pal();
            if (start_game)
            {
                int num = 10;
                if (start_game_flash <= 10)
                {
                    num = ((start_game_flash > 5) ? 2 : ((start_game_flash > 0) ? 1 : 0));
                }
                else if (frames % 10 < 5)
                {
                    num = 7;
                }

                if (num < 10)
                {
                    E.pal(6, num);
                    E.pal(12, num);
                    E.pal(13, num);
                    E.pal(5, num);
                    E.pal(1, num);
                    E.pal(7, num);
                }
            }

            int num2 = 0;
            if (flash_bg)
            {
                num2 = frames / 5;
            }
            else if (new_bg)
            {
                num2 = 2;
            }

            E.rectfill(0f, 0f, 128f, 128f, num2);
            if (!is_title())
            {
                foreach (Cloud cloud in clouds)
                {
                    cloud.x += cloud.spd;
                    E.rectfill(cloud.x, cloud.y, cloud.x + cloud.w, cloud.y + 4f + (1f - cloud.w / 64f) * 12f, (!new_bg) ? 1 : 14);
                    if (cloud.x > 128f)
                    {
                        cloud.x = 0f - cloud.w;
                        cloud.y = E.rnd(120f);
                    }
                }
            }

            E.map(room.X * 16, room.Y * 16, 0, 0, 16, 16, 2);
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject classicObject = objects[i];
                if (classicObject != null && (classicObject is platform || classicObject is big_chest))
                {
                    draw_object(classicObject);
                }
            }

            int tx = (is_title() ? (-4) : 0);
            E.map(room.X * 16, room.Y * 16, tx, 0, 16, 16, 1);
            for (int j = 0; j < objects.Count; j++)
            {
                GameObject classicObject2 = objects[j];
                if (classicObject2 != null && !(classicObject2 is platform) && !(classicObject2 is big_chest))
                {
                    draw_object(classicObject2);
                }
            }

            E.map(room.X * 16, room.Y * 16, 0, 0, 16, 16, 3);
            foreach (Particle particle in particles)
            {
                particle.x += particle.spd;
                particle.y += E.sin(particle.off);
                particle.off += E.min(0.05f, particle.spd / 32f);
                E.rectfill(particle.x, particle.y, particle.x + (float)particle.s, particle.y + (float)particle.s, particle.c);
                if (particle.x > 132f)
                {
                    particle.x = -4f;
                    particle.y = E.rnd(128f);
                }
            }

            for (int num3 = dead_particles.Count - 1; num3 >= 0; num3--)
            {
                DeadParticle deadParticle = dead_particles[num3];
                deadParticle.x += deadParticle.spd.X;
                deadParticle.y += deadParticle.spd.Y;
                deadParticle.t--;
                if (deadParticle.t <= 0)
                {
                    dead_particles.RemoveAt(num3);
                }

                E.rectfill(deadParticle.x - (float)(deadParticle.t / 5), deadParticle.y - (float)(deadParticle.t / 5), deadParticle.x + (float)(deadParticle.t / 5), deadParticle.y + (float)(deadParticle.t / 5), 14 + deadParticle.t % 2);
            }

            E.rectfill(-5f, -5f, -1f, 133f, 0f);
            E.rectfill(-5f, -5f, 133f, -1f, 0f);
            E.rectfill(-5f, 128f, 133f, 133f, 0f);
            E.rectfill(128f, -5f, 133f, 133f, 0f);
            if (is_title())
            {
                E.print("press button", 42f, 96f, 5f);
            }

            if (level_index() != 30)
            {
                return;
            }

            GameObject classicObject3 = null;
            foreach (GameObject @object in objects)
            {
                if (@object is player)
                {
                    classicObject3 = @object;
                    break;
                }
            }

            if (classicObject3 != null)
            {
                float num4 = E.min(24f, 40f - E.abs(classicObject3.x + 4f - 64f));
                E.rectfill(0f, 0f, num4, 128f, 0f);
                E.rectfill(128f - num4, 0f, 128f, 128f, 0f);
            }
        }

        
        public void draw_object(GameObject obj)
        {
            obj.draw();
        }

        
        
        public void draw_time(int x, int y)
        {
            int num = seconds;
            int num2 = minutes % 60;
            int num3 = E.flr(minutes / 60);
            E.rectfill(x, y, x + 32, y + 6, 0f);
            E.print(((num3 < 10) ? "0" : "") + num3 + ":" + ((num2 < 10) ? "0" : "") + num2 + ":" + ((num < 10) ? "0" : "") + num, x + 1, y + 1, 7f);
        }

        
        
        public float clamp(float val, float a, float b)
        {
            return E.max(a, E.min(b, val));
        }

        
        
        public float appr(float val, float target, float amount)
        {
            if (!(val > target))
            {
                return E.min(val + amount, target);
            }

            return E.max(val - amount, target);
        }

        
        
        public int sign(float v)
        {
            if (!(v > 0f))
            {
                if (!(v < 0f))
                {
                    return 0;
                }

                return -1;
            }

            return 1;
        }

        
        
        public bool maybe()
        {
            return E.rnd(1f) < 0.5f;
        }

        
        public bool solid_at(float x, float y, float w, float h)
        {
            return tile_flag_at(x, y, w, h, 0);
        }

        
        public bool ice_at(float x, float y, float w, float h)
        {
            return tile_flag_at(x, y, w, h, 4);
        }

        
        
        public bool tile_flag_at(float x, float y, float w, float h, int flag)
        {
            for (int i = (int)E.max(0f, E.flr(x / 8f)); (float)i <= E.min(15f, (x + w - 1f) / 8f); i++)
            {
                for (int j = (int)E.max(0f, E.flr(y / 8f)); (float)j <= E.min(15f, (y + h - 1f) / 8f); j++)
                {
                    if (E.fget(tile_at(i, j), flag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        
        
        public int tile_at(int x, int y)
        {
            return E.mget(room.X * 16 + x, room.Y * 16 + y);
        }

        
        
        public bool spikes_at(float x, float y, int w, int h, float xspd, float yspd)
        {
            for (int i = (int)E.max(0f, E.flr(x / 8f)); (float)i <= E.min(15f, (x + (float)w - 1f) / 8f); i++)
            {
                for (int j = (int)E.max(0f, E.flr(y / 8f)); (float)j <= E.min(15f, (y + (float)h - 1f) / 8f); j++)
                {
                    int num = tile_at(i, j);
                    if (num == 17 && (E.mod(y + (float)h - 1f, 8f) >= 6f || y + (float)h == (float)(j * 8 + 8)) && yspd >= 0f)
                    {
                        return true;
                    }

                    if (num == 27 && E.mod(y, 8f) <= 2f && yspd <= 0f)
                    {
                        return true;
                    }

                    if (num == 43 && E.mod(x, 8f) <= 2f && xspd <= 0f)
                    {
                        return true;
                    }

                    if (num == 59 && ((x + (float)w - 1f) % 8f >= 6f || x + (float)w == (float)(i * 8 + 8)) && xspd >= 0f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        
        public CalGame()
        {
        }
    }
}
