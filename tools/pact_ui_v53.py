# v0.0.53 pact UI fix: bakes the APPROVED placeholder art (step57 main menu, step60 pact panels) into
# Resources/HellordArt/PactUI at 4x (480x270 mockup space -> 1920x1080) so the game matches the mockups.
import sys, os, math, random
sys.path.insert(0, '/workspace/Hellord/Art/previews/step60_pact_icons_clarity')
from PIL import Image, ImageDraw
src = open('/workspace/Hellord/Art/previews/step60_pact_icons_clarity/v15_mockups.py').read().split('# ---------- compact pact panel')[0]
g = {}; exec(src, g)
for k, v in g.items(): globals().setdefault(k, v)
OUT = '/workspace/Hellord/Assets/Hellord/Resources/HellordArt/PactUI/'
def save4(im, n): im.resize((im.width * 4, im.height * 4), Image.NEAREST).save(OUT + n + '.png')
def piece(w, h): im = Image.new('RGBA', (w, h), (0, 0, 0, 0)); return im, ImageDraw.Draw(im)

# ---- main menu background (step57 00_main_menu minus dynamic parts) ----
im = hellbg()
for bx in (24, 446): im = glowspot(im, bx + 7, 206, 26, (140, 60, 10), 0.8)
im = glowspot(im, 150, 52, 60, (110, 20, 20), 0.6)
im = glowspot(im, 152, 93, 40, (140, 40, 10), 0.7)  # DESCEND glow
d = ImageDraw.Draw(im)
for bx in (24, 446): brazier(d, bx, 196)
for i, (sx, sy) in enumerate([(60, 244), (80, 248), (395, 246), (418, 242), (240, 250), (255, 247)]): skull(d, sx, sy, SAND if i % 2 else GREY)
d.polygon([(52, 22), (36, 4), (64, 18)], fill=BROWN); d.polygon([(248, 22), (264, 4), (236, 18)], fill=BROWN)
outlined(d, (152, 40), "HELLORD", f(40, True), RED, INK, 2)
d.text((152, 40), "HELLORD", font=f(40, True), fill=RED, anchor="mm"); d.text((152, 38), "HELLORD", font=f(40, True), fill=(255, 90, 80), anchor="mm")
d.text((152, 64), "— survive the descent —", font=f(8), fill=AMB, anchor="mm")
d.rectangle((318, 6, 474, 22), fill=INK, outline=BROWN)
d.rectangle((0, 256, 480, 270), fill=INK)
save4(im, 'pactui_menu_bg')
# ---- buttons ----
im, d = piece(174, 26)
d.polygon([(10, 0), (162, 0), (172, 11), (162, 24), (10, 24), (0, 11)], fill=DRED, outline=AMB); d.polygon([(12, 2), (160, 2), (168, 9), (4, 9)], fill=RED)
outlined(d, (86, 13), "DESCEND", f(14, True), YEL); save4(im, 'pactui_btn_descend')
for nm in ["SACRIFICE ALTAR", "CODEX", "SETTINGS", "QUIT"]:
    for on in (0, 1):
        im, d = piece(138, 18)
        d.polygon([(6, 0), (130, 0), (136, 8), (130, 16), (6, 16), (0, 8)], fill=(92, 40, 48) if on else PLUM, outline=AMB if on else BROWN)
        d.text((68, 8), nm, font=f(10, True), fill=YEL if on else SAND, anchor="mm")
        save4(im, 'pactui_btn_' + nm.split()[0].lower() + ('_on' if on else ''))
im, d = piece(12, 12); skull(d, 1, 0, SAND); save4(im, 'pactui_skull')
im, d = piece(12, 14); d.polygon([(0, 6), (10, 0), (10, 12)], fill=AMB); save4(im, 'pactui_arrow')
im, d = piece(12, 12); d.text((6, 6), "✦", font=f(10), fill=AMB, anchor="mm"); save4(im, 'pactui_soul')
im, d = piece(12, 12); d.text((6, 6), "◆", font=f(10), fill=(181, 80, 136), anchor="mm"); save4(im, 'pactui_sigil')
# ---- pact ring 0..6 ----
for n in range(7):
    im, d = piece(124, 124); cx = cy = 62
    d.ellipse((cx - 55, cy - 55, cx + 55, cy + 55), fill=(INK[0], INK[1], INK[2], 238))  # B11: opaque disc so castle windows don't show through
    d.ellipse((cx - 54, cy - 54, cx + 54, cy + 54), outline=DRED, width=2); d.ellipse((cx - 46, cy - 46, cx + 46, cy + 46), outline=BROWN)
    for k in range(5):
        a1 = math.radians(-90 + k * 144); a2 = math.radians(-90 + (k + 2) * 144)
        d.line((cx + 46 * math.cos(a1), cy + 46 * math.sin(a1), cx + 46 * math.cos(a2), cy + 46 * math.sin(a2)), fill=BROWN)
    for i in range(6):
        a = math.radians(-90 + i * 60); x = cx + 50 * math.cos(a); y2 = cy + 50 * math.sin(a); s = i < n
        d.ellipse((x - 8, y2 - 8, x + 8, y2 + 8), fill=AMB if s else INK, outline=RED if s else SLATE, width=2)
    outlined(d, (cx, cy - 6), "PACT", f(11, True), AMB); outlined(d, (cx, cy + 8), f"{n}/6", f(14, True), YEL)
    save4(im, f'pactui_ring_{n}')
# ---- altar ----
im = hellbg(5); im = Image.blend(im, Image.new("RGB", im.size, INK), 0.55); save4(im, 'pactui_altar_bg')
for i in range(4):
    for dim in (0, 1):
        im, d = piece(24, 24); icon(d, i, 0, 0, 24, bool(dim)); save4(im, f'pactui_icon_{i}' + ('_dim' if dim else ''))
im, d = piece(22, 22); d.rectangle((9 - 2, 12 - 2, 17 - 2, 19 - 2), fill=GREY); d.arc((10 - 2, 6 - 2, 16 - 2, 15 - 2), 180, 0, fill=GREY, width=2); save4(im, 'pactui_lock')
im, d = piece(22, 22); d.text((11, 11), "✓", font=f(14), fill=INK, anchor="mm"); save4(im, 'pactui_check')
for st, fill, ol in (("lock", (50, 46, 60), SLATE), ("own", INK, AMB), ("sel", YEL, AMB), ("pact", INK, YEL)):
    w = 32 if st == "pact" else 22; im, d = piece(w + 1, w + 1)
    d.polygon([(w / 2, 0), (w, w / 2), (w / 2, w), (0, w / 2)], fill=fill, outline=ol); save4(im, 'pactui_key_' + st)
im, d = piece(98, 26)
d.polygon([(8, 0), (88, 0), (96, 12), (88, 24), (8, 24), (0, 12)], fill=DRED, outline=AMB); outlined(d, (48, 12), "SEAL PACT", f(13, True), YEL); save4(im, 'pactui_btn_seal')
im, d = piece(98, 26)
d.polygon([(8, 0), (88, 0), (96, 12), (88, 24), (8, 24), (0, 12)], fill=PLUM, outline=SLATE); d.text((48, 12), "SEAL PACT", font=f(13, True), fill=GREY, anchor="mm"); save4(im, 'pactui_btn_seal_off')
print('ok')
