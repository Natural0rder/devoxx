sp.listConnections();

var source = {
    $source:
    {
        connectionName: 'OrdersStream',
        db: 'devoxx',
        coll: 'orders',
        timeField: '$fullDocument.timestamp',
        config: {
            fullDocument: 'whenAvailable'
        }
    }
};

var pipeline = [
    { $sort: { "fullDocument.timestamp": -1 } },
    { $limit: 1000 },
    { $unwind: "$fullDocument.products" },
    {
        $project: {
            storeId: "$fullDocument.storeId",
            saleAmount: {
                $multiply: [
                    "$fullDocument.products.quantity",
                    { $toDecimal: "$fullDocument.products.price" }
                ]
            },
            quantity: "$fullDocument.products.quantity"
        }
    },
    {
        $group: {
            _id: "$storeId",
            totalSales: { $sum: "$saleAmount" },
            totalQuantity: { $sum: "$quantity" }
        }
    },
    {
        $project: {
            _id: "$_id",
            totalSales: 1,
            avgSalePrice: {
                $cond: [
                    { $eq: ["$totalQuantity", 0] },
                    0,
                    {
                        $round: [
                            { $divide: ["$totalSales", "$totalQuantity"] },
                            2
                        ]
                    }
                ]
            }
        }
    }
];

var window = {
    $tumblingWindow: {
        interval: { size: NumberInt(5), unit: 'second' },
        pipeline: pipeline
    }
};

var sink = {
    $merge: {
        into: {
            connectionName: 'OrdersStream',
            db: 'devoxx',
            coll: 'stats'
        },
        on: ['_id'],
        whenMatched: 'merge',
        whenNotMatched: 'insert'
    }
};

sp.process([source, window, sink]);

sp.createStreamProcessor("computeStats", [source, window, sink]);

sp.computeStats.start();