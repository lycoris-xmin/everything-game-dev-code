#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
国风 HD-2D 像素素材生成器 v2（无外网依赖，纯 PIL）。

核心改进：
- 所有精灵先画色块再自动加 1px 深色描边，剪影清晰。
- 背景改用数学曲线（正弦叠峦）+ 国风剪影（亭/塔/松），不再随机三角。
- 严格限制调色板与明暗：左上主光、右下暗部、统一描边。

运行：python tools/gen_pixel_art.py
"""
import os, math, random
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
A = os.path.join(ROOT, "assets")
P = os.path.join(A, "pixel")
os.makedirs(P, exist_ok=True)

# ---- 国风调色板（有限、协调、可复用）----------------------------------------
C = {
    "O": (0, 0, 0, 0),
    # 天空/远山
    "sky0": (18, 22, 46), "sky1": (38, 44, 84), "sky2": (72, 70, 112),
    "moon": (255, 248, 220), "moon_g": (200, 204, 230), "star": (230, 236, 255),
    "mfar": (52, 60, 110), "mfar_d": (34, 40, 80),
    "mmid": (42, 94, 102), "mmid_d": (26, 64, 70),
    "mnear": (18, 44, 42), "mnear_d": (10, 28, 28),
    "mist": (170, 196, 208),
    # 角色
    "ink": (22, 26, 36),
    "skin": (250, 210, 168), "skin_d": (210, 155, 118),
    "hair": (42, 34, 52), "hair_d": (26, 20, 34),
    "robe": (230, 240, 248), "robe_d": (160, 178, 196), "robe_s": (120, 136, 154),
    "sash": (176, 52, 52), "sash_d": (120, 34, 34),
    "gold": (212, 168, 84), "gold_d": (148, 108, 48),
    "jade": (108, 188, 168), "jade_d": (64, 126, 112),
    "steel": (200, 208, 218), "steel_d": (140, 150, 162),
    # 妖兽
    "fur": (120, 126, 140), "fur_d": (76, 82, 96), "fur_l": (166, 172, 186),
    "vine": (80, 158, 98), "vine_d": (46, 108, 70), "leaf": (110, 188, 126),
    "spider": (72, 62, 100), "spider_d": (44, 36, 70),
    "eye": (232, 64, 54),
}


def new(w, h):
    return Image.new("RGBA", (w, h), C["O"])


def save(im, path):
    im.save(path)
    print("  ->", os.path.relpath(path, ROOT))


def rect(d, x0, y0, x1, y1, c, ol=None):
    d.rectangle([x0, y0, x1, y1], fill=c, outline=ol)


def ell(d, x0, y0, x1, y1, c, ol=None):
    d.ellipse([x0, y0, x1, y1], fill=c, outline=ol)


def poly(d, pts, c, ol=None):
    d.polygon(pts, fill=c, outline=ol)


def line(d, x0, y0, x1, y1, c, w=1):
    d.line([x0, y0, x1, y1], fill=c, width=w)


# ---- 通用：自动 1px 深色描边 -------------------------------------------------
def outline(im, col=C["ink"]):
    px = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a > 20:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h:
                        if px[nx, ny][3] < 20:
                            px[nx, ny] = col + (255,)
    return im


# ---- 通用：给图层底部加落地椭圆投影 -----------------------------------------
def cast_shadow(d, cx, cy, rx, ry, alpha=120):
    ell(d, cx - rx, cy - ry, cx + rx, cy + ry, (12, 16, 24, alpha))


# ===== 角色：剑修 云无月 =====================================================
def hero():
    im = new(28, 44)
    d = ImageDraw.Draw(im)

    # 投影
    cast_shadow(d, 14, 40, 9, 3)

    # 腿（被袍子遮住，下摆）
    rect(d, 9, 30, 12, 38, C["robe_s"])
    rect(d, 16, 30, 19, 38, C["robe_s"])

    # 长袍主体（梯形，左亮右暗）
    poly(d, [(8, 16), (20, 16), (22, 35), (6, 35)], C["robe"])
    # 右暗部
    poly(d, [(17, 16), (20, 16), (22, 35), (17, 35)], C["robe_d"])
    # 左亮部
    poly(d, [(8, 16), (12, 16), (9, 35), (6, 35)], C["robe"])
    # 下摆暗
    rect(d, 6, 32, 22, 35, C["robe_s"])
    # 腰封
    rect(d, 7, 26, 21, 29, C["sash"])
    rect(d, 12, 26, 16, 29, C["gold"])
    # 衣褶
    line(d, 12, 18, 11, 34, C["robe_d"], 1)
    line(d, 17, 18, 18, 33, C["robe_s"], 1)

    # 双臂（左亮右暗）
    rect(d, 6, 17, 9, 27, C["robe"])
    rect(d, 7, 17, 8, 27, C["robe_d"])
    rect(d, 19, 17, 22, 27, C["robe_d"])
    rect(d, 19, 17, 20, 27, C["robe_s"])

    # 头部
    ell(d, 9, 5, 19, 15, C["skin"])
    ell(d, 9, 11, 19, 15, C["skin_d"])
    # 头发（大包+鬓发）
    ell(d, 8, 1, 17, 7, C["hair"])          # 顶髻
    rect(d, 8, 4, 10, 14, C["hair_d"])      # 左鬓
    rect(d, 18, 5, 20, 13, C["hair"])       # 右发
    rect(d, 9, 3, 19, 7, C["hair"])         # 刘海

    # 眉眼（细长丹凤）
    line(d, 11, 9, 13, 9, C["ink"], 1)
    line(d, 15, 9, 17, 9, C["ink"], 1)
    d.point((12, 10), C["ink"])
    d.point((16, 10), C["ink"])

    # 剑（背后斜插，左上→右下）
    line(d, 21, 4, 26, 32, C["steel_d"], 3)
    line(d, 21, 4, 26, 32, C["steel"], 1)
    rect(d, 23, 27, 27, 30, C["gold"])      # 剑格
    rect(d, 24, 30, 26, 34, C["hair_d"])    # 柄

    outline(im)
    return im


# ===== 妖兽 1：石毛狼 =======================================================
def enemy_wolf():
    im = new(36, 24)
    d = ImageDraw.Draw(im)
    cast_shadow(d, 18, 21, 12, 3)

    # 后腿/前腿
    rect(d, 7, 16, 10, 23, C["fur_d"])
    rect(d, 26, 16, 29, 23, C["fur_d"])
    # 躯干
    poly(d, [(6, 16), (30, 16), (32, 22), (4, 22)], C["fur_d"])
    poly(d, [(6, 16), (22, 16), (24, 22), (4, 22)], C["fur"])
    # 背脊亮线
    line(d, 6, 17, 24, 17, C["fur_l"], 1)

    # 头颈（狼头朝右）
    poly(d, [(22, 8), (34, 10), (33, 18), (21, 18)], C["fur"])
    poly(d, [(26, 8), (34, 10), (33, 14), (26, 14)], C["fur_d"])
    # 尖耳
    poly(d, [(30, 8), (33, 2), (34, 10)], C["fur_d"])
    # 嘴吻
    poly(d, [(32, 12), (36, 14), (33, 16)], C["fur_d"])
    # 妖红眼
    d.point((30, 12), C["eye"])
    d.point((31, 12), C["eye"])
    # 獠牙
    d.point((34, 15), C["gold"])
    d.point((35, 15), C["gold"])
    # 尾
    line(d, 5, 18, 1, 13, C["fur_d"], 3)
    line(d, 5, 18, 1, 13, C["fur"], 1)

    outline(im)
    return im


# ===== 妖兽 2：藤妖 =========================================================
def enemy_vine():
    im = new(30, 32)
    d = ImageDraw.Draw(im)
    cast_shadow(d, 15, 29, 9, 3)

    # 主干（扭曲）
    poly(d, [(10, 12), (20, 12), (22, 30), (8, 30)], C["vine"])
    poly(d, [(16, 12), (20, 12), (22, 30), (16, 30)], C["vine_d"])

    # 上卷须
    line(d, 10, 14, 5, 5, C["vine"], 3)
    line(d, 20, 14, 26, 6, C["vine_d"], 3)
    ell(d, 2, 2, 8, 8, C["leaf"])
    ell(d, 23, 3, 29, 9, C["leaf"])

    # 叶片
    for (x, y) in [(8, 16), (20, 18), (6, 22), (22, 24)]:
        ell(d, x, y, x + 5, y + 4, C["leaf"])
    # 妖眼
    d.point((12, 18), C["eye"])
    d.point((17, 18), C["eye"])
    # 嘴（裂开）
    line(d, 11, 23, 18, 23, C["vine_d"], 2)

    outline(im)
    return im


# ===== 妖兽 3：紫纹蛛 =======================================================
def enemy_spider():
    im = new(30, 28)
    d = ImageDraw.Draw(im)
    cast_shadow(d, 15, 25, 9, 3)

    # 腿（左侧 4 条，右侧 4 条）
    leg = [(-10, -6), (-12, 0), (-10, 6), (-4, 10)]
    for dy, (lx, ly) in enumerate(leg):
        y = 16 + dy * 2
        # 左
        line(d, 15, 18, 15 + lx, y + ly, C["spider_d"], 2)
        # 右
        line(d, 15, 18, 15 - lx, y + ly, C["spider_d"], 2)

    # 腹部
    ell(d, 8, 12, 22, 24, C["spider"])
    ell(d, 8, 16, 22, 24, C["spider_d"])
    ell(d, 8, 12, 14, 24, C["spider"])
    line(d, 15, 13, 15, 23, C["spider_d"], 1)
    # 头胸
    ell(d, 11, 5, 19, 13, C["spider_d"])
    # 妖眼
    for dx in (-2, 2):
        d.point((15 + dx, 8), C["eye"])
        d.point((15 + dx, 10), C["eye"])

    outline(im)
    return im


# ===== Boss：朱甲妖王 ========================================================
def boss():
    im = new(44, 52)
    d = ImageDraw.Draw(im)
    cast_shadow(d, 22, 48, 14, 4)

    # 下半身长袍
    poly(d, [(10, 22), (34, 22), (38, 46), (6, 46)], C["sash_d"])
    poly(d, [(10, 22), (24, 22), (26, 46), (6, 46)], C["sash"])
    # 下摆暗
    rect(d, 6, 42, 38, 46, C["sash_d"])
    # 腰甲
    rect(d, 8, 32, 36, 36, C["gold"])
    rect(d, 8, 32, 36, 34, C["gold_d"])

    # 上身肩甲
    ell(d, 8, 20, 18, 30, C["gold"])
    ell(d, 26, 20, 36, 30, C["gold"])
    ell(d, 26, 22, 34, 30, C["gold_d"])

    # 头
    ell(d, 16, 8, 28, 20, C["skin"])
    rect(d, 16, 16, 28, 20, C["skin_d"])

    # 双角
    poly(d, [(16, 10), (10, 1), (18, 7)], C["gold_d"])
    poly(d, [(28, 10), (34, 1), (26, 7)], C["gold_d"])

    # 怒眉 + 妖眼
    line(d, 16, 12, 20, 14, C["ink"], 2)
    line(d, 28, 12, 24, 14, C["ink"], 2)
    rect(d, 17, 14, 20, 16, C["eye"])
    rect(d, 24, 14, 27, 16, C["eye"])
    # 獠牙
    d.point((20, 18), C["gold"])
    d.point((24, 18), C["gold"])

    outline(im)
    return im


# ===== 地形瓦片（16x16）=====================================================
def grass():
    im = new(16, 16); d = ImageDraw.Draw(im)
    rect(d, 0, 0, 15, 15, (56, 104, 64))
    rect(d, 0, 0, 15, 2, (78, 132, 86))
    rect(d, 0, 13, 15, 15, (36, 76, 46))
    random.seed(7)
    for _ in range(10):
        x = random.randint(1, 14); y = random.randint(3, 14)
        d.line([x, y, x, y - 2], fill=C["leaf"], width=1)
    save(im, os.path.join(P, "tile_grass.png"))


def path():
    im = new(16, 16); d = ImageDraw.Draw(im)
    rect(d, 0, 0, 15, 15, (132, 124, 98))
    rect(d, 0, 0, 15, 2, (162, 154, 126))
    rect(d, 0, 13, 15, 15, (104, 96, 74))
    for (x, y) in [(3, 3), (10, 5), (6, 10), (12, 12), (4, 13)]:
        rect(d, x, y, x + 1, y + 1, (104, 96, 74))
    save(im, os.path.join(P, "tile_path.png"))


def water():
    im = new(16, 16); d = ImageDraw.Draw(im)
    rect(d, 0, 0, 15, 15, (46, 108, 142))
    rect(d, 0, 0, 15, 2, (84, 158, 192))
    for y in (4, 8, 12):
        d.line([1, y, 14, y], fill=(108, 182, 212), width=1)
    save(im, os.path.join(P, "tile_water.png"))


def sand():
    im = new(16, 16); d = ImageDraw.Draw(im)
    rect(d, 0, 0, 15, 15, (168, 160, 120))
    rect(d, 0, 0, 15, 2, (198, 190, 150))
    rect(d, 0, 13, 15, 15, (140, 132, 98))
    save(im, os.path.join(P, "tile_sand.png"))


def tree():
    im = new(24, 32); d = ImageDraw.Draw(im)
    cast_shadow(d, 12, 29, 9, 2)
    rect(d, 10, 18, 14, 30, C["hair_d"])
    ell(d, 2, 2, 22, 18, C["mnear"])
    ell(d, 2, 2, 14, 14, C["leaf"])
    for (x, y) in [(6, 6), (15, 8), (10, 12)]:
        ell(d, x, y, x + 4, y + 4, C["jade"])
    outline(im)
    save(im, os.path.join(P, "tree.png"))


def house():
    im = new(28, 28); d = ImageDraw.Draw(im)
    cast_shadow(d, 14, 26, 11, 2)
    rect(d, 6, 13, 22, 26, C["robe"])
    rect(d, 6, 13, 14, 26, (245, 248, 250))
    # 飞檐
    poly(d, [(2, 13), (26, 13), (22, 5), (6, 5)], C["jade_d"])
    poly(d, [(2, 13), (8, 5), (6, 5)], C["jade"])
    poly(d, [(26, 13), (20, 5), (22, 5)], C["jade"])
    rect(d, 6, 5, 22, 7, C["jade"])
    # 门
    rect(d, 11, 17, 17, 26, C["sash"])
    rect(d, 13, 17, 15, 26, C["sash_d"])
    # 窗
    d.point((9, 16), C["gold_d"]); d.point((19, 16), C["gold_d"])
    outline(im)
    save(im, os.path.join(P, "house.png"))


def rock():
    im = new(16, 16); d = ImageDraw.Draw(im)
    cast_shadow(d, 8, 13, 6, 2)
    poly(d, [(3, 13), (8, 3), (13, 13)], C["fur"])
    poly(d, [(3, 10), (13, 10), (13, 13), (3, 13)], C["fur_d"])
    poly(d, [(3, 3), (8, 3), (6, 10), (3, 10)], C["fur_l"])
    outline(im)
    save(im, os.path.join(P, "rock.png"))


def flower():
    im = new(16, 16); d = ImageDraw.Draw(im)
    rect(d, 7, 9, 9, 15, C["vine_d"])
    for ang in range(0, 360, 72):
        a = math.radians(ang)
        x = 8 + int(4 * math.cos(a)); y = 7 + int(4 * math.sin(a))
        ell(d, x - 2, y - 2, x + 2, y + 2, C["sash"])
    ell(d, 6, 5, 10, 9, C["gold"])
    outline(im)
    save(im, os.path.join(P, "flower.png"))


# ===== 图标 ==================================================================
def ic_atk():
    im = new(32, 32); d = ImageDraw.Draw(im)
    poly(d, [(16, 2), (21, 15), (11, 15)], C["steel"])
    poly(d, [(16, 2), (21, 15), (16, 15)], C["steel_d"])
    rect(d, 11, 15, 21, 17, C["gold"])
    rect(d, 15, 17, 17, 27, C["hair_d"])
    d.point((16, 28), C["gold"])
    outline(im)
    return im


def ic_skill():
    im = new(32, 32); d = ImageDraw.Draw(im)
    ell(d, 6, 6, 26, 26, (60, 150, 200, 40), C["jade"])
    ell(d, 11, 11, 21, 21, C["jade"], C["jade_d"])
    line(d, 14, 16, 22, 16, C["robe"], 2)
    line(d, 16, 13, 16, 19, C["robe"], 2)
    outline(im)
    return im


def ic_ult():
    im = new(32, 32); d = ImageDraw.Draw(im)
    ell(d, 4, 4, 28, 28, C["ink"], C["gold"])
    d.pieslice([4, 4, 28, 28], 90, 270, fill=C["robe"])
    d.pieslice([4, 4, 28, 28], -90, 90, fill=C["ink"])
    ell(d, 14, 4, 18, 8, C["robe"])
    ell(d, 14, 24, 18, 28, C["ink"])
    return im


def mat_wind():
    im = new(16, 16); d = ImageDraw.Draw(im)
    ell(d, 4, 4, 12, 12, C["leaf"])
    rect(d, 4, 4, 8, 12, C["vine_d"])
    line(d, 6, 4, 6, 12, C["vine"], 1)
    return im


def sword():
    im = new(16, 16); d = ImageDraw.Draw(im)
    line(d, 8, 2, 8, 11, C["steel_d"], 4)
    line(d, 8, 2, 8, 11, C["steel"], 2)
    rect(d, 4, 11, 11, 13, C["gold"])
    rect(d, 7, 13, 9, 15, C["hair_d"])
    return im


# ===== 分层背景（480x270）====================================================
def _mountain_layer(d, base_y, color, amp, wave, count, seed):
    random.seed(seed)
    pts = [(0, base_y)]
    x = 0
    while x < 480:
        h = random.randint(*amp)
        x += random.randint(*wave)
        pts.append((x, base_y - h))
    pts.append((480, base_y))
    pts.append((480, 270))
    pts.append((0, 270))
    poly(d, pts, (*color[:3], 255))


def bg_sky():
    im = new(480, 270); d = ImageDraw.Draw(im)
    for y in range(270):
        t = y / 270
        r = int(C["sky0"][0] + (C["sky2"][0] - C["sky0"][0]) * t)
        g = int(C["sky0"][1] + (C["sky2"][1] - C["sky0"][1]) * t)
        bl = int(C["sky0"][2] + (C["sky1"][2] - C["sky0"][2]) * t)
        d.line([(0, y), (479, y)], fill=(r, g, bl))
    # 月光晕（实心圆环，避免透明羽化）
    for rad, a in [(64, 45), (48, 90), (36, 160)]:
        ell(d, 380 - rad, 70 - rad, 380 + rad, 70 + rad, (*C["moon_g"][:3], a))
    ell(d, 356, 46, 404, 94, C["moon"])
    # 星点
    random.seed(3)
    for _ in range(100):
        x = random.randint(0, 479); y = random.randint(0, 150)
        d.point((x, y), C["star"])
    save(im, os.path.join(P, "bg_sky.png"))
    return im


def bg_far():
    im = new(480, 270); d = ImageDraw.Draw(im)
    _mountain_layer(d, 150, C["mfar"], (35, 80), (50, 90), 8, 11)
    _mountain_layer(d, 170, C["mfar_d"], (20, 55), (60, 100), 7, 13)
    save(im, os.path.join(P, "bg_far.png"))
    return im


def bg_mid():
    im = new(480, 270); d = ImageDraw.Draw(im)
    _mountain_layer(d, 200, C["mmid"], (25, 65), (70, 110), 6, 21)
    # 山腰雾
    rect(d, 0, 192, 479, 204, (*C["mist"][:3], 140))

    # 朱红亭台（中景，剪影式）
    hx, hy = 110, 200
    rect(d, hx, hy - 28, hx + 40, hy, C["sash_d"])
    rect(d, hx + 2, hy - 28, hx + 22, hy, C["sash"])
    # 顶
    poly(d, [(hx - 6, hy - 28), (hx + 46, hy - 28), (hx + 40, hy - 40), (hx, hy - 40)], C["jade_d"])
    poly(d, [(hx - 6, hy - 28), (hx + 8, hy - 40), (hx, hy - 40)], C["jade"])
    poly(d, [(hx + 46, hy - 28), (hx + 32, hy - 40), (hx + 40, hy - 40)], C["jade"])
    # 柱/门洞
    rect(d, hx + 16, hy - 28, hx + 24, hy, C["sash_d"])

    # 远塔（多重檐剪影）
    tx, ty = 360, 200
    rect(d, tx, ty - 52, tx + 14, ty, C["mnear_d"])
    poly(d, [(tx - 4, ty - 52), (tx + 18, ty - 52), (tx + 7, ty - 66)], C["mnear_d"])
    poly(d, [(tx - 2, ty - 36), (tx + 16, ty - 36), (tx + 7, ty - 42)], C["mnear"])
    poly(d, [(tx - 1, ty - 18), (tx + 15, ty - 18), (tx + 7, ty - 24)], C["mnear"])

    save(im, os.path.join(P, "bg_mid.png"))
    return im


def bg_near():
    im = new(480, 270); d = ImageDraw.Draw(im)
    _mountain_layer(d, 250, C["mnear"], (15, 45), (80, 130), 6, 31)

    # 雾松剪影（近景）
    def pine(x, y):
        rect(d, x, y - 52, x + 5, y, C["mnear_d"])
        for ly in range(y - 52, y - 4, 8):
            w = max(3, 18 - (y - ly) // 5)
            d.line([(x + 2, ly), (x + 2 - w, ly + 10)], fill=C["mnear_d"], width=2)
            d.line([(x + 2, ly), (x + 2 + w, ly + 10)], fill=C["mnear_d"], width=2)

    for tx in (30, 180, 320, 430):
        pine(tx, 262)

    # 底部雾带
    rect(d, 0, 252, 479, 270, (*C["mist"][:3], 130))
    save(im, os.path.join(P, "bg_near.png"))
    return im


def world_back():
    sky = bg_sky()
    far = bg_far()
    out = Image.new("RGBA", (480, 270))
    out.paste(sky, (0, 0))
    out.paste(far, (0, 0), far)
    save(out, os.path.join(P, "world_back.png"))
    return out


# ===== 国风纹样 ==============================================================
def motif_cloud():
    im = new(48, 24); d = ImageDraw.Draw(im)
    ell(d, 4, 8, 20, 20, C["gold"], C["gold_d"])
    ell(d, 16, 4, 34, 16, C["gold"], C["gold_d"])
    ell(d, 30, 10, 44, 22, C["gold"], C["gold_d"])
    line(d, 4, 14, 44, 14, C["gold_d"], 2)
    outline(im)
    save(im, os.path.join(P, "motif_cloud.png"))
    return im


def motif_seal():
    im = new(40, 40); d = ImageDraw.Draw(im)
    rect(d, 2, 2, 37, 37, C["sash"], C["sash_d"])
    rect(d, 6, 6, 33, 33, C["sash"])
    # 篆字暗示（井/田）
    line(d, 13, 10, 13, 30, C["robe"], 3)
    line(d, 26, 10, 26, 30, C["robe"], 3)
    line(d, 10, 16, 30, 16, C["robe"], 3)
    line(d, 10, 24, 30, 24, C["robe"], 3)
    save(im, os.path.join(P, "motif_seal.png"))
    return im


def motif_bagua():
    im = new(32, 32); d = ImageDraw.Draw(im)
    ell(d, 1, 1, 31, 31, C["gold"], C["gold_d"])
    ell(d, 4, 4, 28, 28, C["ink"])
    ell(d, 14, 4, 18, 28, C["ink"])
    ell(d, 4, 14, 28, 18, C["ink"])
    ell(d, 13, 13, 19, 19, C["gold"])
    save(im, os.path.join(P, "motif_bagua.png"))
    return im


# ===== 主程序 ================================================================
def main():
    print("生成国风 HD-2D 像素素材 v2 ->")
    save(hero(), os.path.join(A, "hero.png"))
    save(enemy_wolf(), os.path.join(A, "enemy_wolf.png"))
    save(enemy_vine(), os.path.join(A, "enemy_vine.png"))
    save(enemy_spider(), os.path.join(A, "enemy_spider.png"))
    save(boss(), os.path.join(A, "boss.png"))
    grass(); path(); water(); sand(); tree(); house(); rock(); flower()
    save(ic_atk(), os.path.join(A, "ic_atk.png"))
    save(ic_skill(), os.path.join(A, "ic_skill.png"))
    save(ic_ult(), os.path.join(A, "ic_ult.png"))
    save(mat_wind(), os.path.join(A, "mat_wind.png"))
    save(sword(), os.path.join(A, "sword.png"))
    bg_sky(); bg_far(); bg_mid(); bg_near(); world_back()
    motif_cloud(); motif_seal(); motif_bagua()
    print("完成。")


if __name__ == "__main__":
    main()
