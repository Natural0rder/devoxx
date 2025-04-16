function randomStatus() {
  const statuses = ['PENDING', 'SHIPPED', 'DELIVERED', 'CANCELLED'];
  return statuses[Math.floor(Math.random() * statuses.length)];
}

function randomEan(length) {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
  let ean = '';
  for (let i = 0; i < length; i++) {
    ean += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return ean;
}

function randomDecimal(min, max) {
  return Number((Math.random() * (max - min) + min).toFixed(2));
}

function randomQuantity(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function randomDate(start, end) {
  return new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
}

function randomStoreId() {
  return Math.floor(Math.random() * 10) + 1; // 1 to 10
}

function generateProducts() {
  const count = randomQuantity(1, 100);
  const products = [];
  for (let i = 0; i < count; i++) {
    products.push({
      ean: randomEan(13),
      quantity: randomQuantity(1, 100),
      price: NumberDecimal(randomDecimal(10, 1000).toString())
    });
  }
  return products;
}

// Insert random documents
for (let i = 0; i < 1000; i++) {
  db.orders.insertOne({
    deliveryDate: randomDate(new Date("2025-01-01"), new Date("2025-12-31")),
    status: randomStatus(),
    storeId: randomStoreId(),
    products: generateProducts(),
    timestamp: new Date()
  });
}

