# t1263: builds the runtime talisman textures from HA's hand-pixelled frames (Art/previews/t1263_talisman).
#   Resources/HellordArt/ui_t1263_talisman_frames.png  176x32 RGBA, 8 frames of 22x32 side by side (sRGB, Point, no compression, PPU 1)
#   Resources/HellordArt/ui_t1263_talisman_masks.png   176x32 RGB, R = ink_mask, G = paper_mask, B = alpha_mask (linear, Point)
import glob, os
from PIL import Image
root = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
art = os.path.join(root, "Art", "previews", "t1263_talisman"); out = os.path.join(root, "Assets", "Hellord", "Resources", "HellordArt")
fr = sorted(glob.glob(os.path.join(art, "native", "talisman_sway_f0?_*ms_22x32.png"))); assert len(fr) == 8
F = Image.new("RGBA", (176, 32), (0, 0, 0, 0)); M = Image.new("RGB", (176, 32), (0, 0, 0))
for i, f in enumerate(fr):
    im = Image.open(f).convert("RGBA"); assert im.size == (22, 32)
    px = im.load()
    for y in range(32):
        for x in range(22):
            if px[x, y][3] == 0: px[x, y] = (0, 0, 0, 0)
    F.paste(im, (i * 22, 0))
    m = [Image.open(os.path.join(art, "masks", "%s_mask_f%02d_22x32.png" % (n, i))).convert("L").point(lambda v: 255 if v > 127 else 0) for n in ("ink", "paper", "alpha")]
    # NOTE: HA's paper_mask files also contain the dark outline pixels (110 px per frame, colour (24,20,37)); the README says they must stay unlit, so they are removed here.
    pm = m[1].load()
    for y in range(32):
        for x in range(22):
            if px[x, y][3] and px[x, y][:3] == (24, 20, 37): pm[x, y] = 0
    M.paste(Image.merge("RGB", m), (i * 22, 0))
F.save(os.path.join(out, "ui_t1263_talisman_frames.png")); M.save(os.path.join(out, "ui_t1263_talisman_masks.png")); print("ok")
