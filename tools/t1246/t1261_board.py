# t1261 previews: composes labelled boards from the in-game native captures. usage: t1261_board.py <capture_dir> <out_dir>
import sys, os
from PIL import Image, ImageDraw
src, out = sys.argv[1], sys.argv[2]; os.makedirs(out, exist_ok=True)
LBL = "IN-GAME CAPTURE (preview, not packaged)"
def label(im, text):
    W = max(im.width, 8*len(text)+8); c = Image.new("RGB", (W, im.height+22), (18, 16, 24)); c.paste(im, (0, 22)); ImageDraw.Draw(c).text((4, 5), text, fill=(246,238,222)); return c
def nat(n): return Image.open(os.path.join(src, n)).convert("RGB")
# full x3
full = Image.open(os.path.join(src, "t1261_full.png")).convert("RGB"); label(full, LBL + " - Sins screen x3 (1440x810)").save(os.path.join(out, "sins_full_x3_ingame.png"))
# slot row + header, 8x. slot row x184..296 y18..50 ; header plaque x285..416 y54..71
n0 = nat("t1261_empty_style0.png"); n1 = nat("t1261_empty_style1.png")
def crop8(im, b, k=8): return im.crop(b).resize(((b[2]-b[0])*k, (b[3]-b[1])*k), Image.NEAREST)
row0 = crop8(n0, (180, 14, 300, 54)); row1 = crop8(n1, (180, 14, 300, 54)); hdr = crop8(n0, (280, 50, 420, 76))
W = max(row0.width, hdr.width); H = row0.height*2 + hdr.height + 3*22 + 20
b = Image.new("RGB", (W, H), (18, 16, 24)); d = ImageDraw.Draw(b); y = 0
for t, im in (("A (default): faint socket glyph + tiny EMPTY, 8x", row0), ("B: tiny EMPTY only, 8x", row1), ("Header plaque OWNED / SLOTS, 8x", hdr)):
    d.text((4, y+5), LBL + " - " + t, fill=(246,238,222)); b.paste(im, (0, y+22)); y += im.height + 28
b.crop((0, 0, W, y)).save(os.path.join(out, "slot_row_and_header_8x_ingame.png"))
# detail boards 4x: panel x293..416 y75..230
def panel(n): return crop8(nat("t1261_detail_" + n + ".png"), (293, 75, 417, 231), 4)
def board(names, titles, fn, head):
    ims = [panel(n) for n in names]; w = ims[0].width; h = ims[0].height
    B = Image.new("RGB", (len(ims)*(w+12)+12, h+52), (18, 16, 24)); d = ImageDraw.Draw(B); d.text((8, 6), LBL + " - " + head, fill=(246,238,222))
    for i, (im, t) in enumerate(zip(ims, titles)): B.paste(im, (12+i*(w+12), 40)); d.text((12+i*(w+12), 26), t, fill=(200,190,170))
    B.save(os.path.join(out, fn))
board(["mortal","demonic","hd","locked","equipped"], ["Mortal (owned)","Demonic","Heaven-Defying","Locked","Equipped"], "detail_panel_board_4x_ingame.png", "detail panel 4x")
board(["longest","longest_p2"], ["Longest stats p1","p2 (pager)"], "detail_panel_longest_4x_ingame.png", "longest stat list, Fire Wheel of Nezha's Ghost, 4x")
