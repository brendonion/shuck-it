extends Node

var payment

var save_system

var ads_button

var dialog

var type = "inapp"

var sku = "ad_removal"

func _ready():
  if Engine.has_singleton("GodotGooglePlayBilling"):
    payment     = Engine.get_singleton("GodotGooglePlayBilling")
    save_system = get_parent().find_node("SaveSystem")
    ads_button  = get_parent().find_node("AdsButton")
    dialog      = get_parent().find_node("AcceptDialog")
    dialog.get_close_button().visible = false

    # These are all signals supported by the API
    # You can drop some of these based on your needs
    payment.connect("connected", self, "_on_connected") # No params
    payment.connect("disconnected", self, "_on_disconnected") # No params
    payment.connect("connect_error", self, "_on_connect_error") # Response ID (int), Debug message (string)
    payment.connect("purchases_updated", self, "_on_purchases_updated") # Purchases (Dictionary[])
    payment.connect("purchase_error", self, "_on_purchase_error") # Response ID (int), Debug message (string)
    payment.connect("sku_details_query_completed", self, "_on_sku_details_query_completed") # SKUs (Dictionary[])
    payment.connect("sku_details_query_error", self, "_on_sku_details_query_error") # Response ID (int), Debug message (string), Queried SKUs (string[])
    payment.connect("purchase_acknowledged", self, "_on_purchase_acknowledged") # Purchase token (string)
    payment.connect("purchase_acknowledgement_error", self, "_on_purchase_acknowledgement_error") # Response ID (int), Debug message (string), Purchase token (string)
    # payment.connect("purchase_consumed", self, "_on_purchase_consumed") # Purchase token (string)
    # payment.connect("purchase_consumption_error", self, "_on_purchase_consumption_error") # Response ID (int), Debug message (string), Purchase token (string)

    payment.startConnection()
  else:
    print("Android IAP support is not enabled. Make sure you have enabled 'Custom Build' and the GodotGooglePlayBilling plugin in your Android export settings! IAP will not work.")

### Event callbacks ###

func _on_connected():
  print("GodotGooglePlayBilling CONNECTED")
  payment.querySkuDetails([self.sku], self.type)

func _on_disconnected():
  print("GodotGooglePlayBilling DISCONNECTED")

func _on_connect_error(response_id, message):
  print("GodotGooglePlayBilling CONNECT ERROR")
  print("Response ID: " + response_id)
  print("Message: " + message)

func _on_purchases_updated(purchases):
  print("GodotGooglePlayBilling PURCHASE UPDATED")
  print("Purchases: ", purchases)
  check_purchase()

func _on_purchase_error(response_id, message):
  print("GodotGooglePlayBilling PURCHASE ERROR")
  print("Response ID: " + str(response_id))
  print("Message: " + message)

func _on_sku_details_query_completed(sku_details):
  print("GodotGooglePlayBilling SKU DETAILS QUERY COMPLETED")
  print("Sku Details: ", sku_details)

func _on_sku_details_query_error(response_id, message, skus):
  print("GodotGooglePlayBilling SKU DETAILS QUERY ERROR")
  print("Response ID: " + str(response_id))
  print("Message: " + message)
  for sku_name in skus:
        print("SKU: "+ sku_name)

func _on_purchase_acknowledged(purchase_token):
  print("GodotGooglePlayBilling PURCHASE ACKNOWLEDGED")
  print("Purchase Token: " + purchase_token)

func _on_purchase_acknowledgement_error(response_id, message, purchase_token):
  print("GodotGooglePlayBilling PURCHASE ACKNOWLEDGMENT ERROR")
  print("Response ID: " + str(response_id))
  print("Message: " + message)
  print("Purchase Token: " + purchase_token)
  dialog.dialog_text = "Something went wrong with your purchase."
  dialog.popup_centered()
  save_system.enableAds = true
  save_system.Save()
  # Update UI
  ads_button.visible = true

### Helper functions ###

func purchase_item():
  print("PURCHASING ITEM...")
  payment.purchase(self.sku)

func check_purchase():
  print("CHECKING PURCHASE...")
  var query = payment.queryPurchases(self.type) # Or "subs" for subscriptions
  print("Purchase Query: ", query);
  if query.status == OK:
    print("PURCHASE OK.")
    if query.purchases == null || query.purchases.size() == 0:
      save_system.enableAds = true
      save_system.Save()
      # Update UI
      ads_button.visible = true
    else:
      for purchase in query.purchases:
          # If purchase sku matches self.sku AND purchase state != Pending
          if purchase.sku == self.sku && purchase.purchase_state != 2:
              # Entitle the user to the content they bought
              save_system.enableAds = false
              save_system.Save()
              # Update UI
              ads_button.visible = false
              if !purchase.is_acknowledged:
                  payment.acknowledgePurchase(purchase.purchase_token)
          elif purchase.purchase_state == 2:
            dialog.dialog_text = "Purchase pending."
            dialog.popup_centered()
  else:
    print("PURCHASE ERROR.")
