# t1262 previews: before/after braziers from in-game native captures. usage: t1262_board.py <old_dir> <new_dir> <out_dir>
import sys, os
from PIL import Image, ImageDraw
old, new, out = sys.argv[1:4]; os.makedirs(out, exist_ok=True)
LBL = "IN-GAME CAPTURE (preview, not packaged)"
def crop(d, i, b, k): im = Image.open(os.path.join(d, "braz_%d.png" % i)).convert("RGB").crop(b); return im.resize((im.width*k, im.height*k), Image.NEAREST)
def board(b, k, fn, title, n=3, cols=("BEFORE (t1260)", "AFTER (t1262)")):
    w, h = (b[2]-b[0])*k, (b[3]-b[1])*k; B = Image.new("RGB", (n*(w+10)+10, 2*(h+22)+30), (18,16,24)); d = ImageDraw.Draw(B); d.text((8,6), LBL + " - " + title, fill=(246,238,222))
    for r, dd in enumerate((old, new)):
        for i in range(n):
            y = 24 + r*(h+22); B.paste(crop(dd, i, b, k), (10+i*(w+10), y+14)); d.text((10+i*(w+10), y), "%s  flicker frame %d" % (cols[r], i+1), fill=(200,190,170))
    B.save(os.path.join(out, fn))
board((0, 176, 40, 240), 8, "brazier_left_8x_before_after_3_flicker_frames.png", "left brazier 8x, 3 flicker frames")
board((440, 176, 480, 240), 8, "brazier_right_8x_before_after_3_flicker_frames.png", "right brazier 8x, 3 flicker frames")
board((0, 150, 48, 250), 4, "pillar_left_4x_before_after.png", "left pillar 4x (light on stone)", 2)
