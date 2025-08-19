using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities {
    
    [CustomEntity("DBBHelper/Rope")]
    class Rope:Entity{

//-------------设置-------------
        public bool  start_point_fixation=false;//指示首端节点位置是否固定
        public bool end_point_fixation=false;//指示末端节点位置是否固定
        public bool absorption_mode=false;//吸附模式

//-------------节点情况-------------
        private List<Vector2>all_pos=new List<Vector2>();//本次节点位置
        private List<Vector2>all_pos_swap=new List<Vector2>();//临时节点位置，用于中间交换
        private List<Vector2> all_previous_pos=new List<Vector2>();//上次节点位置
        private List<Vector2>all_a=new List<Vector2>();//节点加速度

//-------------杂项-------------
        private List<MTexture> textures=new List<MTexture>();//所有纹理
        private Color rope_color=Color.White;//绳子外观颜色
        private Color inner_color=Color.RosyBrown;//绳子内芯颜色

//-------------物理参数-------------
        public int node_num=20;//节点数目，不得小于2
        public int iter_number=20;//方程组迭代次数
        private float rope_length=8.0f; //绳子相邻节点的标准长度
        public float g=9.8f;//重力加速度
        public float stiffness=1.0f;//刚性系数，范围在(0,1]，值越小代表绳子越有弹性
        public float damping=0.8f;//阻尼系数，用于空气阻力
        public float tensile_coefficient=1.2f;//抗拉系数，当两端均吸附在物体上时，当绳子长度超出绳子 正常长度*抗拉系数 上时，绳子末端脱落
//-------------行为控制参数-------------
        private bool start_absorption=false;//首端是否被吸附
        private bool start_first_absoprtion=true;//首端在上一次吸附之后是否第一次被吸附
        private Solid start=null;//首端吸附的物体
        private Vector2 start_pos_previous;//上一次首端吸附时实体的位置
        private Vector2 start_pos;//这一次首端吸附时实体的位置


        private bool end_absorption=false;//尾端是否被吸附
        private bool end_first_absoprtion=true;//尾端在上一次吸附之后是否第一次被吸附
        private Solid end=null;//尾端吸附的物体
        private Vector2 end_pos_previous;//上一次尾端吸附时实体的位置
        private Vector2 end_pos;//这一次尾端吸附时实体的位置

        public Rope(EntityData data,Vector2 offset)
        {
            this.Position=data.Position+offset;
            this.node_num=data.Int("Resolution")+2;
            this.iter_number=data.Int("IterationNumber");

            this.g=data.Float("GravitityAcceleration");
            this.stiffness=data.Float("Stiffness");
            this.damping=data.Float("Damping");
            this.tensile_coefficient=data.Float("TensileCoefficient");
            
            this.start_point_fixation=data.Bool("StartPointFixation");
            this.end_point_fixation=data.Bool("EndPointFixation");
            this.absorption_mode=data.Bool("AbsorptionMode");

            Vector2 dir=data.Nodes[0]-data.Position;
            this.rope_length=dir.Length()/(node_num-1);
            //初始化绳子各节点的位置
            for(int i=0;i<node_num;i++)
            {
                this.all_pos.Add(Position+((float)i/(float)node_num)*dir);
                this.all_pos_swap.Add(all_pos[i]);
                this.all_previous_pos.Add(all_pos[i]);
                this.all_a.Add(new Vector2(0,0));
            }
            //初始化纹理
            string start_texture_path=data.Attr("StartTexturePath");
            string middle_texture_path=data.Attr("MiddleTexturePath");
            string end_texture_path=data.Attr("EndTexturePath");
            if(GFX.Game.Has(start_texture_path)==false){this.textures.Add(GFX.Game["objects/DBB_Items/Rope/rope_start00"]);}
            else{this.textures.Add(GFX.Game[start_texture_path]);}

            if(GFX.Game.Has(middle_texture_path)==false){this.textures.Add(GFX.Game["objects/DBB_Items/Rope/rope_middle00"]);}
            else{this.textures.Add(GFX.Game[middle_texture_path]);}

            if(GFX.Game.Has(end_texture_path)==false){this.textures.Add(GFX.Game["objects/DBB_Items/Rope/rope_end00"]);}
            else{this.textures.Add(GFX.Game[end_texture_path]);}
            
            rope_color=data.HexColor("OuterColor");rope_color.A=(byte)(255*data.Float("OuterColorAlpha"));
            inner_color=data.HexColor("InnerColor");inner_color.A=(byte)(255*data.Float("InnerColorAlpha"));

            //初始化深度
            if(data.Attr("Depth")=="BG(9100)"){this.Depth=9100;}
            else{this.Depth=-10600;}

            
        }
        //构造器
        public Rope(Vector2 start_position,Vector2 end_position,int Resolution,int iter_number,float GravitityAcceleration,float Stiffness,float Damping,bool start_point_fixation
        ,bool end_point_fixation,bool absorption_mode,bool bg=true,MTexture rope_start_texture=null,MTexture rope_texture=null,MTexture rope_end_texture=null)
        {
            this.Position=start_position;
            this.node_num=Resolution+2;
            this.iter_number=iter_number;

            this.g=GravitityAcceleration;
            this.stiffness=Stiffness;
            this.damping=Damping;
            
            this.start_point_fixation=start_point_fixation;
            this.end_point_fixation=end_point_fixation;
            this.absorption_mode=absorption_mode;

            Vector2 dir=end_position-start_position;
            this.rope_length=dir.Length()/(node_num-1);
            //初始化绳子各节点的位置
            for(int i=0;i<node_num;i++)
            {
                this.all_pos.Add(Position+((float)i/(float)node_num)*dir);
                this.all_pos_swap.Add(all_pos[i]);
                this.all_previous_pos.Add(all_pos[i]);
                this.all_a.Add(new Vector2(0,0));
            }
            //初始化纹理
            if(rope_start_texture==null){this.textures.Add(GFX.Game["objects/Rope/rope_start00"]);}
            else{this.textures.Add(rope_start_texture);}

            if(rope_texture==null){this.textures.Add(GFX.Game["objects/Rope/rope_middle00"]);}
            else{this.textures.Add(rope_texture);};
            
            if(rope_end_texture==null){this.textures.Add(GFX.Game["objects/Rope/rope_end00"]);}
            else{this.textures.Add(rope_end_texture);}
            //初始化深度
            if(bg==true){this.Depth=9100;}
            else{this.Depth=-10600;}

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        //端点吸附检测
        private void AbsorptionCheck()
        {
            //如果start_point_fixation已被设置，则绳子首端将不受吸附模式影响，同时不与任何实体进行交互
            if(start_point_fixation==true)
            {
                start_absorption=true;
            }
            //如果吸附模式开
            else if(absorption_mode==true)
            {
                //检测吸附物体
                start=Scene.CollideFirst<Solid>(new Rectangle((int)all_pos[0].X-2, (int)all_pos[0].Y-2,4,4));
                //如果吸附到了实体上
                if(start!=null)
                {
                    //检测绳子长度，对于末端end_point_fixation为真且被拉伸过长的情况，默认首端无法吸附在实体上
                    float length=0.0f;
                    for(int i=0;i<all_pos.Count-1;i++)
                    {
                        length+=(all_pos[i]-all_pos[i+1]).Length();
                    }
                    if(end_point_fixation==true && length>tensile_coefficient*rope_length*(all_pos.Count-1))
                    {
                        start_absorption=false;
                    }
                    //成功吸附
                    else
                    {
                        start_absorption=true;
                        //如果是第一次被吸附，则初始化两个位置项
                        if(start_first_absoprtion==true)
                        {
                            start_pos=start.Position;
                            start_pos_previous=start_pos;
                            start_first_absoprtion=false;
                        }
                        //如果不是第一次被吸附，则更新两个位置项
                        else
                        {
                            start_pos_previous=start_pos;
                            start_pos=start.Position;
                        }
                    }
                   
                }
                //如果没有吸附到实体上
                else
                {
                    //初始化start_absorption和start_first_absoprtion为下一次吸附做准备
                    start_absorption=false;
                    start_first_absoprtion=true;
                }
            }
            //如果end_point_fixation已被设置，则绳子末端将不受吸附模式影响，同时不与任何实体进行交互
            if(end_point_fixation==true)
            {
                end_absorption=true;
            }
            //如果吸附模式开
            else if(absorption_mode==true)
            {
                //检测吸附物体
                end=Scene.CollideFirst<Solid>(new Rectangle((int)all_pos[all_pos.Count-1].X-2, (int)all_pos[all_pos.Count-1].Y-2,4,4));
                //如果可能吸附到了实体上
                if(end!=null)
                {
                    //检测绳子长度，对于首端被吸附且被拉伸过长的情况，默认末端无法吸附在实体上
                    float length=0.0f;
                    for(int i=0;i<all_pos.Count-1;i++)
                    {
                        length+=(all_pos[i]-all_pos[i+1]).Length();
                    }
                    if(start_absorption==true && length>tensile_coefficient*rope_length*(all_pos.Count-1))
                    {
                        end_absorption=false;
                    }
                    //成功吸附
                    else
                    {
                        end_absorption=true;
                        //如果是第一次被吸附，则初始化两个位置项
                        if(end_first_absoprtion==true)
                        {
                            end_pos=end.Position;
                            end_pos_previous=end_pos;
                            end_first_absoprtion=false;
                        }
                        //如果不是第一次被吸附，则更新两个位置项
                        else
                        {   
                            end_pos_previous=end_pos;
                            end_pos=end.Position;
                        }
                    }
                   
                }
                else
                {
                    //初始化end_absorption和end_first_absoprtion为下一次吸附做准备
                    end_absorption=false;
                    end_first_absoprtion=true;
                }
            }
            
        }
        //更新节点
        private void UpdateNode()
        {
            //进行吸附检测
            AbsorptionCheck();
            //更新各个节点的位置
            UpdateNodePosition();
            //松弛，对绳子节点的位置进行修正
            //首先将约束方程组线性化，接着将欠定方程组化为标准方程组
            //本质上是雅各比迭代求解线性方程组
            //性能损耗项，迭代次数过多游戏性能会有明显下降
            Relaxation();
            //更新加速度
            UpdateAcceleration();
            
        }
        private void UpdateNodePosition()
        {
            //Verlet方法更新节点的位置
            //对于首尾节点，根据吸附检测的结果进行处理
            //如果首端吸附了
            if(start_absorption==true)
            {
                //如果start为null，代表为start_point_fixation为真的情况，此时首端节点不与任何实体交互
                //如果start不为null，代表为start_point_fixation为假的情况，此时首端节点已经吸附到实体上了，应进行处理
                if(start!=null)
                {
                    all_pos[0]+=start_pos-start_pos_previous;
                    all_pos_swap[0]=all_pos[0];
                    all_previous_pos[0]=all_pos[0];
                }
            }
            //如果首端未吸附
            else
            {
                all_pos_swap[0]=all_pos[0];
                all_pos[0]=2*all_pos[0]-all_previous_pos[0]+Engine.RawDeltaTime*Engine.RawDeltaTime*all_a[0];
                all_previous_pos[0]=all_pos_swap[0];
            }

            //如果尾端吸附了
            int last_index=all_pos.Count-1;
            if(end_absorption==true)
            {
                //如果end为null，代表为end_point_fixation为真的情况，此时首端节点不与任何实体交互
                //如果end不为null，代表为end_point_fixation为假的情况，此时首端节点已经吸附到实体上了，应进行处理
                if(end!=null)
                {
                    
                    all_pos[last_index]+=end_pos-end_pos_previous;
                    all_pos_swap[last_index]=all_pos[last_index];
                    all_previous_pos[last_index]=all_pos[last_index];
                }
            }
            //如果尾端未吸附
            else
            {
                all_pos_swap[last_index]=all_pos[last_index];
                all_pos[last_index]=2*all_pos[last_index]-all_previous_pos[last_index]+Engine.RawDeltaTime*Engine.RawDeltaTime*all_a[last_index];
                all_previous_pos[last_index]=all_pos_swap[last_index];
            }
            //对于中间节点，正常更新位置
            for(int i=all_pos.Count-2;i>0;i--)
            {
                //更新节点的位置
                all_pos_swap[i]=all_pos[i];
                all_pos[i]=2*all_pos[i]-all_previous_pos[i]+Engine.RawDeltaTime*Engine.RawDeltaTime*all_a[i];
                all_previous_pos[i]=all_pos_swap[i];
                //更新节点速度
            }
        }
        //更新重力
        private void UpdateGravity(int index)
        {
            //赋予重力加速度
            all_a[index]=new Vector2(0,g);
            //对于首尾节点，如果固定，则重力加速度设置为0
            if(index==0)
            {
                if(start_absorption==true)
                {
                    all_a[index]=new Vector2(0,0);
                }
            }
            else if(index==all_a.Count-1)
            {
                if(end_absorption==true)
                {
                    all_a[index]=new Vector2(0,0);
                }
            }
        }
        //更新风力
        private void UpdateWindForce(int index)
        {
            Random r=new Random();
            float variant=1.0f*r.NextSingle();
            Level level = (Scene as Level);
            if (index == 0)
            {
                if (start_absorption == false)
                {
                    //风力
                    all_a[index] += level.Wind * (variant + 1.0f);
                }
            }
            else if (index == all_a.Count - 1)
            {
                if (end_absorption == false)
                {
                    //风力
                    all_a[index] += level.Wind * (variant + 1.0f);
                }
            }
            else
            {
                //风力
                all_a[index] += level.Wind;

            }
        }
        //更新空气阻力
        private void UpdateAirResistance(int index)
        {
            //注意使用物理帧而不是游戏帧，Engine.DeltaTime是有可能掉到0的
            if (index == 0)
            {
                if (start_absorption == false)
                {
                    //空气阻力
                    all_a[index] -= damping * (all_pos[index] - all_previous_pos[index]) / Engine.RawDeltaTime;
                }
            }
            else if (index == all_a.Count - 1)
            {
                if (end_absorption == false)
                {
                    //空气阻力
                    all_a[index] -= damping * (all_pos[index] - all_previous_pos[index]) / Engine.RawDeltaTime;
                }
            }
            else
            {
                //空气阻力
                all_a[index] -= damping * (all_pos[index] - all_previous_pos[index]) / Engine.RawDeltaTime;
            }
        }
        private void UpdateBuoyancy(int index)
        {
            
            if (index == 0)
            {
                if (start_absorption == false)
                {
                    //空气阻力
                    all_a[index] -= damping * (all_pos[index] - all_previous_pos[index]) / Engine.RawDeltaTime;
                }
            }
            else if (index == all_a.Count - 1)
            {
                if (end_absorption == false)
                {
                    //空气阻力
                    all_a[index] -= damping * (all_pos[index] - all_previous_pos[index]) / Engine.RawDeltaTime;
                }
            }
            else
            {
                //空气阻力
                all_a[index] -= damping * (all_pos[index] - all_previous_pos[index]) / Engine.RawDeltaTime;
            }
        }
        //更新加速度
        private void UpdateAcceleration()
        {
            //更新加速度
            for (int index = all_pos.Count - 1; index >= 0; index--)
            {
                //更新重力
                UpdateGravity(index);
                //更新风力
                UpdateWindForce(index);
                //更新空气阻力
                UpdateAirResistance(index);
            }
        }
        private void Relaxation()
        {
            Vector2 direction;//相邻节点的方向
            float delta_length;
            //更新iter_number轮
            for (int j = 0; j < iter_number; j++)
            {
                //对于第二个节点和第一个节点，如果start_absorption为假则正常松弛，否则仅对第二个节点的一端进行松弛
                direction = all_pos[0] - all_pos[1];
                delta_length = stiffness * (direction.Length() - rope_length);
                direction.Normalize();

                all_pos[1] += delta_length * direction;
                if (start_absorption == false)
                {
                    all_pos[0] -= delta_length * direction;
                }

                //对于非第二个，非倒数第二个，非第一个和非最后一个节点而言，两端同时松弛
                for (int i = 2; i < all_pos.Count - 1; i++)
                {
                    direction = all_pos[i - 1] - all_pos[i];
                    delta_length = stiffness * 0.5f * (direction.Length() - rope_length);
                    direction.Normalize();
                    all_pos[i - 1] -= delta_length * direction;
                    all_pos[i] += delta_length * direction;
                }

                //对于倒数第二个节点和最后一个节点，如果end_absorption为假则正常松弛，否则仅对倒数第二个节点的一端进行松弛
                direction = all_pos[all_pos.Count - 2] - all_pos[all_pos.Count - 1];
                delta_length = stiffness * 0.5f * (direction.Length() - rope_length);
                direction.Normalize();
                all_pos[all_pos.Count - 2] -= delta_length * direction;
                if (end_absorption == false)
                {
                    all_pos[all_pos.Count - 1] += delta_length * direction;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            UpdateNode();
            RemoveCheck();  
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);    
        }
        //绳索移除检测，如果绳索所有节点都在房间外，则移除
        private void RemoveCheck()
        {
            //决定是否移除绳索
            bool decision = true;
            //检测所有节点的位置，如果它们均在房间边界范围外，则移除该绳索
            for (int i = 0; i < all_pos.Count; i++)
            {
                if ((Scene as Level).IsInBounds(all_pos[i]))
                {
                    decision = false;
                }
            }
            if (decision == true)
            {
                this.RemoveSelf();
            }
        }
        public override void Render()
        {
            base.Render();
            int last_index=all_pos.Count-1;
            for(int i=0;i<last_index;i++)
            {
                Draw.Line(all_pos[i],all_pos[i+1],inner_color,2.0f);
                
                Vector2 tex_pos=(all_pos[i]+all_pos[i+1])/2.0f;
                Vector2 tex_dir=all_pos[i]-all_pos[i+1];
                float tex_s=tex_dir.Length()/textures[1].Width;
                tex_dir.Normalize();
                textures[1].DrawCentered(tex_pos,rope_color,new Vector2(tex_s,1.0f),tex_dir.Angle());
                if(i==0)
                {
                    textures[0].DrawCentered(all_pos[i],rope_color,1.0f,tex_dir.Angle());
                }
                if(i==last_index-1)
                {
                    textures[2].DrawCentered(all_pos[i+1],rope_color,1.0f,tex_dir.Angle());
                }
                
            }
        }
    }


}
