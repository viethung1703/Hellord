"""t1356 check (placement only): min distance from each Souls/Sigils plate to the arch / pillars / title, from mt_scene1356 geometry."""
import sys; sys.dont_write_bytecode = True
sys.path.insert(0, '/workspace/HellordRepo/tools/t1356')
import mt_scene1356 as M, numpy as np
from scipy import ndimage as ndi
L, R = M.PLATE_L, M.PLATE_R; M.PLATE_L = M.PLATE_R = -300
rgba, info = M.build(); mask = rgba[..., 3] == 255; mask[118:, 45:595] = False
title = mask.copy(); mask[M.TITLE_Y - 3:M.TITLE_Y + 14, 240:400] = False
d = ndi.distance_transform_edt(~mask)
for nm, x in (('L', L), ('R', R)): print('plate', nm, 'clear of arch/pillars', d[M.PLATE_Y:M.PLATE_Y + M.PLATE_H, x:x + M.PLATE_W].min())
print('gap to title: left', 320 - M.S.TITLE.width('INFERNAL SINS') // 2 - (L + M.PLATE_W), 'right', R - (320 + (M.S.TITLE.width('INFERNAL SINS') + 1) // 2))
