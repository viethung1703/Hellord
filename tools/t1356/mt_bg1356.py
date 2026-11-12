"""t1356: uniform dim of the approved v5 bg1 lava for the menu-theme Sins screen, at the same brightness as the Pacts screen background (mean ~ pm_bg).
The old v6 'dimB' background had dark zones baked around the OLD text positions (bgproc5 zones), which showed as black patches once the text moved.
c' = DEEP + 0.5 * (c - DEEP) per colour, no zones, no snap (10 source colours stay 10 colours). Writes MenuTheme/mt_bg.png only."""
import numpy as np
from PIL import Image
raw = np.array(Image.open('/workspace/Hellord/Art/previews/t1287_redraw/D2_MORTA_v5/background/V5_bg1_lava_processed_640x360_AI_BG_MOCKUP_NOT_RUNTIME.png').convert('RGB')).astype(float)
B = np.array([14, 10, 20.]); out = np.clip(np.rint(B + 0.5 * (raw - B)), 0, 255).astype(np.uint8)
for o in ('/workspace/HellordRepo', '/workspace/Hellord', '/workspace/HellordProject'):
    Image.fromarray(out).save(o + '/Assets/Hellord/Resources/HellordArt/MenuTheme/mt_bg.png')
print('mean', out.mean(), len(np.unique(out.reshape(-1, 3), axis=0)))
Image.fromarray(out).resize((960, 540), 0).save('/tmp/mtbg_new2.png')
