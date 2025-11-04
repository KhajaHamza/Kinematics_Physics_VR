# VR Teleportation Setup Guide

## Problem Summary
Teleportation works on "Teleport Area" but shows red (invalid) when pointing at "Street road" planes.

## Root Cause
The XR Ray Interactor (Teleport Interactor) is configured with a raycast mask that only includes specific Unity layers. The street road objects must be on a Unity layer that is included in this raycast mask.

## Solution

### Step 1: Check Your Teleport Interactor Raycast Mask

1. In your scene, select the **XR Origin (XR Rig)** GameObject
2. Find **LeftHand Controller** or **RightHand Controller** (whichever has teleportation)
3. Look for the **Teleport Interactor** child GameObject
4. Click on it to view its **XRRayInteractor** component
5. Check the **Raycast Mask** field

Current value: `2147483681` includes:
- Layer 31 (Interaction layer 31, often "Teleport")
- Layer 5 (default UI layer)

### Step 2: Configure Your Street Road Objects

You have two options:

#### Option A: Use Default Layer (Simplest - Recommended)
1. Select each of your 3 street road **plane child objects**
2. In the Inspector, look for the **Layer** dropdown (top of Inspector)
3. Set them to **Default** layer (Layer 0)
4. This layer is typically included in most raycast masks

**Important**: Layer 0 (Default) is likely **NOT** currently included in your teleport raycast mask! Read Step 3 to fix this.

#### Option B: Create a New "Teleport Surface" Layer
1. Go to **Edit > Project Settings > Tags and Layers**
2. Find an empty **Layer** slot (e.g., Layer 8)
3. Name it "TeleportSurface"
4. Select your street road planes and set them to this new layer
5. Skip to Step 4 to add this layer to the raycast mask

### Step 3: Update Teleport Interactor Raycast Mask

The key issue: Your Teleport Interactor's raycast mask currently only includes layers 31 and 5. You need to add the layer your street road planes are on.

**Method A: Add Layer 0 (Default) to Raycast Mask**

1. Select the **Teleport Interactor** GameObject (under LeftHand Controller or RightHand Controller)
2. Find **XRRayInteractor** component
3. In **Raycast Mask**, click to open the layer selection
4. Check **Default** (Layer 0) IN ADDITION to the existing layers
5. Click away to apply

**Method B: Add Your Custom "TeleportSurface" Layer**

1. Follow same steps as Method A
2. Check **TeleportSurface** (or whatever layer you chose)
3. Apply changes

### Step 4: Verify XR Teleportation Area Settings

On each of your 3 street road plane objects:

1. Find the **XR Teleportation Area** component (you already added this)
2. Check **Interaction Layer Mask** is set to **Everything** (or at least includes the same interaction layer as the teleport interactor)
3. Verify the **Interaction Layers** field includes Layer 31 (teleport interaction layer)

### Step 5: Verify Collider Settings

1. Ensure each plane has a **Box Collider** component
2. The collider should **NOT** be marked as **Is Trigger**
3. The collider must have **enabled** checkbox checked

### Step 6: Verify the Working Teleport Area Configuration

Compare your working "Teleport Area":
- Unity Layer: 0 (Default)
- Interaction Layer Mask: Everything (4294967295)
- XR Teleportation Area component: Present and enabled
- Box Collider: Present, enabled, NOT a trigger

Your street road planes should match these settings.

## Recommended Configuration Summary

### For Street Road Planes:
- **GameObject Layer**: 0 (Default) or any layer included in step 3
- **XR Teleportation Area Component**: Present and enabled
- **Interaction Layer Mask**: Everything
- **Box Collider**: Present, enabled, NOT a trigger

### For Teleport Interactor (XRRayInteractor):
- **Raycast Mask**: Must include the layer your street road planes are on

## Testing

After making these changes:
1. Enter Play mode
2. Point your controller at the street road
3. The curve should turn green/blue (valid) instead of red (invalid)
4. You should be able to teleport to the road surface

## Troubleshooting

### Still showing red?
1. Check that the street road GameObject layer is in the Raycast Mask dropdown
2. Verify the collider is enabled and not a trigger
3. Make sure the XR Teleportation Area component is enabled
4. Check that your controller is actually using the teleport interactor (not the regular ray interactor)

### Teleport works but player falls through?
- The plane colliders need to have proper size and be positioned correctly
- Ensure the colliders are not too small

## Layer Configuration Reference

### Teleport Interactor Current Raycast Mask
- Decimal: `2147483681`
- Binary: `1000000000000000000000000100001`
- Includes: Bits 31 and 5 (Layers 31 and 5)

To add Layer 0 (Default):
- Set the raycast mask to include Layer 0 in the dropdown, or
- Use bit mask value: `2147483649` (only bit 0 added) or `4294967295` (everything)

